class SerialCommunication : public ICommunication {
  private:
    bool m_isOpen;

  public:
    SerialCommunication() {
      m_isOpen = false;
    }

    bool isOpen(){
      return m_isOpen;
    }

    void start(){
      //Serial.setTimeout(1000000);
      Serial.begin(SERIAL_BAUD_RATE);
      m_isOpen = true;
    }

    void output(char* data){
      Serial.println(data);
      Serial.flush();
    }

    bool readData(char* input) {
  if (!Serial.available()) return false;  // Skip if nothing to read

  int index = 0;
  while (Serial.available()) {
    char c = Serial.read();
    if (c == '\n') {
      input[index] = '\0';
      return true;
    }
    if (index < 99) {
      input[index++] = c;
    }
  }

  input[index] = '\0';
  return (index > 0);
}

};
