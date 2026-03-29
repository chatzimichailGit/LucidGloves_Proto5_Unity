#if ENABLE_MEDIAN_FILTER
#include <RunningMedian.h>
RunningMedian rmSamples[5] = {
  RunningMedian(MEDIAN_SAMPLES),
  RunningMedian(MEDIAN_SAMPLES),
  RunningMedian(MEDIAN_SAMPLES),
  RunningMedian(MEDIAN_SAMPLES),
  RunningMedian(MEDIAN_SAMPLES)
};
#endif

int maxFingers[5] = {0, 0, 0, 0, 0};
int minFingers[5] = {ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX};
int splayMax[5] = {0, 0, 0, 0, 0};
int splayMin[5] = {ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX};

byte selectPins[] = {PINS_MUX_SELECT};

void setupInputs() {
  Serial.begin(115200);
  Serial.println("Setting up input pins...");

  pinMode(PIN_JOY_BTN, INPUT_PULLUP);
  pinMode(PIN_A_BTN, INPUT_PULLUP);
  pinMode(PIN_B_BTN, INPUT_PULLUP);
  pinMode(PIN_MENU_BTN, INPUT_PULLUP);

  #if !TRIGGER_GESTURE
  pinMode(PIN_TRIG_BTN, INPUT_PULLUP);
  #endif
  #if !GRAB_GESTURE
  pinMode(PIN_GRAB_BTN, INPUT_PULLUP);
  #endif
  #if !PINCH_GESTURE
  pinMode(PIN_PNCH_BTN, INPUT_PULLUP);
  #endif
  #if USING_CALIB_PIN
  pinMode(PIN_CALIB, INPUT_PULLUP);
  #endif

  #if USING_MULTIPLEXER
  for (int i = 0; i < sizeof(selectPins); i++) {
    pinMode(selectPins[i], OUTPUT);
  }
  #endif
}

int analogPinRead(int pin) {
  #if USING_MULTIPLEXER
  if (ISMUX(pin)) {
    return readMux(UNMUX(pin));
  } else {
    return analogRead(pin);
  }
  #else
  return analogRead(pin);
  #endif
}

int readMux(byte pin) {
  int numSelectPins = sizeof(selectPins) / sizeof(selectPins[0]);
  for (int i = numSelectPins - 1; i >= 0; i--) {
    digitalWrite(selectPins[i], (pin & (1 << i)) ? HIGH : LOW);
  }
  delayMicroseconds(5);
  return analogRead(MUX_INPUT);
}

int sinCosMix(int sinPin, int cosPin, int i, bool calibrating) {
  int sinRaw = analogPinRead(sinPin);
  int cosRaw = analogPinRead(cosPin);

  static int sinMin[5] = {ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX};
  static int sinMax[5] = {0, 0, 0, 0, 0};
  static int cosMin[5] = {ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX, ANALOG_MAX};
  static int cosMax[5] = {0, 0, 0, 0, 0};

  if (calibrating) {
    sinMin[i] = min(sinMin[i], sinRaw);
    sinMax[i] = max(sinMax[i], sinRaw);
    cosMin[i] = min(cosMin[i], cosRaw);
    cosMax[i] = max(cosMax[i], cosRaw);
  }

  // Constrain and map
  sinRaw = constrain(sinRaw, sinMin[i], sinMax[i]);
  cosRaw = constrain(cosRaw, cosMin[i], cosMax[i]);

  int sinScaled = map(sinRaw, sinMin[i], sinMax[i], -ANALOG_MAX, ANALOG_MAX);
  int cosScaled = map(cosRaw, cosMin[i], cosMax[i], -ANALOG_MAX, ANALOG_MAX);

  double angleRaw = atan2(sinScaled, cosScaled);
  float normalized = (angleRaw + PI) / (2.0 * PI);
  int result = (int)(normalized * ANALOG_MAX);
  return constrain(result, 0, ANALOG_MAX);
}



int* getFingerPositions(bool calibrating, bool reset) {
  static int fingerPos[10];

int flexRaw[5] = {
  NO_THUMB ? 0 : sinCosMix(PIN_THUMB, PIN_THUMB_SECOND, 0, calibrating),
  sinCosMix(PIN_INDEX, PIN_INDEX_SECOND, 1, calibrating),
  sinCosMix(PIN_MIDDLE, PIN_MIDDLE_SECOND, 2, calibrating),
  sinCosMix(PIN_RING, PIN_RING_SECOND, 3, calibrating),
  0  // placeholder for pinky
};

int splayRaw[5] = {
  NO_THUMB ? 0 : analogPinRead(PIN_THUMB_SPLAY),
  analogPinRead(PIN_INDEX_SPLAY),
  analogPinRead(PIN_MIDDLE_SPLAY),
  analogPinRead(PIN_RING_SPLAY),
  0  // placeholder for pinky
};

// Mirror ring finger values to pinky
flexRaw[4] = flexRaw[3];
splayRaw[4] = splayRaw[3];


#if ENABLE_MEDIAN_FILTER
  for (int i = 0; i < 5; i++) {
    rmSamples[i].add(flexRaw[i]);
    flexRaw[i] = rmSamples[i].getMedian();
  }
#endif

  if (reset) {
    for (int i = 0; i < 5; i++) {
      maxFingers[i] = 0;
      minFingers[i] = ANALOG_MAX;
      splayMax[i] = 0;
      splayMin[i] = ANALOG_MAX;
    }
  }

  if (calibrating) {
    Serial.println("🛠️ Updating calibration min/max...");
    for (int i = 0; i < 5; i++) {
      maxFingers[i] = max(maxFingers[i], flexRaw[i]);
      minFingers[i] = min(minFingers[i], flexRaw[i]);
      splayMax[i] = max(splayMax[i], splayRaw[i]);
      splayMin[i] = min(splayMin[i], splayRaw[i]);
    }
  }

  for (int i = 0; i < 5; i++) {
    // Flexion mapping with clamping
    if (minFingers[i] != maxFingers[i]) {
      int val = map(flexRaw[i], minFingers[i], maxFingers[i], 0, ANALOG_MAX);
      fingerPos[i] = constrain(val, 0, ANALOG_MAX);
    } else {
      fingerPos[i] = ANALOG_MAX / 2;
    }

    // Splay mapping with clamping
    if (splayMin[i] != splayMax[i]) {
      int val = map(splayRaw[i], splayMin[i], splayMax[i], 0, ANALOG_MAX);
      fingerPos[i + 5] = constrain(val, 0, ANALOG_MAX);
    } else {
      fingerPos[i + 5] = ANALOG_MAX / 2;
    }
  }

  // Enhanced filtering with jump rejection near boundaries
  static int previous[10] = {0};
  const int jitterThreshold = 10;
  const int lowerWrapLimit = 600;
  const int upperWrapLimit = ANALOG_MAX - 600;

  for (int i = 0; i < 10; i++) {
    int current = fingerPos[i];
    int last = previous[i];
    int delta = abs(current - last);

    // Block sudden wraparound-type jumps
    bool falseWrapDown = (last > upperWrapLimit && current < lowerWrapLimit);
    bool falseWrapUp   = (last < lowerWrapLimit && current > upperWrapLimit);

    if (falseWrapDown || falseWrapUp) {
      fingerPos[i] = previous[i];  // Ignore the jump
    }
    else if (delta < jitterThreshold) {
      fingerPos[i] = previous[i];  // Smooth normal jitter
    }
    else {
      previous[i] = fingerPos[i];  // Accept change
    }
  }
//delay(100);
  return fingerPos;
}



int analogReadDeadzone(byte pin) {
  int raw = analogRead(pin);
  if (abs(ANALOG_MAX / 2 - raw) < JOYSTICK_DEADZONE * ANALOG_MAX / 100)
    return ANALOG_MAX / 2;
  else
    return raw;
}

int getJoyX() {
  #if JOYSTICK_BLANK
    return ANALOG_MAX / 2;
  #elif JOY_FLIP_X
    return ANALOG_MAX - analogReadDeadzone(PIN_JOY_X);
  #else
    return analogReadDeadzone(PIN_JOY_X);
  #endif
}

int getJoyY() {
  #if JOYSTICK_BLANK
    return ANALOG_MAX / 2;
  #elif JOY_FLIP_Y
    return ANALOG_MAX - analogReadDeadzone(PIN_JOY_Y);
  #else
    return analogReadDeadzone(PIN_JOY_Y);
  #endif
}

bool getButton(byte pin) {
  return digitalRead(pin) != HIGH;
}
