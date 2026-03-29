#define ALWAYS_CALIBRATING false
#define CALIB_OVERRIDE false
#define SERVO_TEST_MODE false  // Set to false to disable


#define ALWAYS_CALIBRATING false
#define CALIB_OVERRIDE false
#define SERVO_TEST_MODE true  // Set to false to disable
#include "ESP32Servo.h"
extern Servo thumbServo;
extern Servo indexServo;
extern Servo middleServo;
extern Servo ringServo;
extern Servo pinkyServo;

ICommunication* comm;
int loops = 0;

void setup() {
  #if COMMUNICATION == COMM_SERIAL
    comm = new SerialCommunication();
  #elif COMMUNICATION == COMM_BTSERIAL
    comm = new BTSerialCommunication();
  #endif  
  comm->start();

  setupInputs();

  #if USING_FORCE_FEEDBACK
    setupServoHaptics();  
/*
    #if SERVO_TEST_MODE
      Serial.println("Starting servo test sweep...");

      for (int angle = 0; angle <= 155; angle += 2) {
        thumbServo.write(angle);
        indexServo.write(angle);
        middleServo.write(angle);
        ringServo.write(angle);
        pinkyServo.write(angle);
        delay(30);  // adjust sweep speed
      }

      for (int angle = 155; angle >= 0; angle -= 2) {
        thumbServo.write(angle);
        indexServo.write(angle);
        middleServo.write(angle);
        ringServo.write(angle);
        pinkyServo.write(angle);
        delay(30);
      }

      Serial.println("Servo test complete.");
    #endif
*/
  #endif
}


void loop() {
  if (comm->isOpen()) {
    // 👇 We force calibration on boot without a button
    bool calibButton = false;
    bool calibrate = false;

    if (loops < CALIBRATION_LOOPS || ALWAYS_CALIBRATING) {
      calibrate = true;
      loops++;
    }

    // 🔁 Clear one-time debug logs
    static bool printedStart = false;
    static bool printedEnd = false;

    if (calibrate && !printedStart) {
      Serial.println("🟢 Calibration started");
      printedStart = true;
      printedEnd = false;
    }

    if (!calibrate && !printedEnd && loops >= CALIBRATION_LOOPS) {
      Serial.println("✅ Calibration finished");
      printedEnd = true;
      printedStart = false;
    }

    // 👇 Actual glove data update
    int* fingerPos = getFingerPositions(calibrate, calibButton);
    bool joyButton = getButton(PIN_JOY_BTN) != INVERT_JOY;

    #if TRIGGER_GESTURE
    bool triggerButton = triggerGesture(fingerPos);
    #else
    bool triggerButton = getButton(PIN_TRIG_BTN) != INVERT_TRIGGER;
    #endif

    bool aButton = getButton(PIN_A_BTN) != INVERT_A;
    bool bButton = getButton(PIN_B_BTN) != INVERT_B;

    #if GRAB_GESTURE
    bool grabButton = grabGesture(fingerPos);
    #else
    bool grabButton = getButton(PIN_GRAB_BTN) != INVERT_GRAB;
    #endif

    #if PINCH_GESTURE
    bool pinchButton = pinchGesture(fingerPos);
    #else
    bool pinchButton = getButton(PIN_PNCH_BTN) != INVERT_PINCH;
    #endif

    bool menuButton = getButton(PIN_MENU_BTN) != INVERT_MENU;

    comm->output(encode(fingerPos, getJoyX(), getJoyY(), joyButton, triggerButton, aButton, bButton, grabButton, pinchButton, calibButton, menuButton));

    #if USING_FORCE_FEEDBACK
    char received[100];
    if (comm->readData(received)) {
      int hapticLimits[5];
      if (String(received).length() >= 10) {
        decodeData(received, hapticLimits);
        writeServoHaptics(hapticLimits); 
      }
    }
    #endif

    delay(LOOP_TIME);
  }
}
