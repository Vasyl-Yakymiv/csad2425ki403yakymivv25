void setup() {
  Serial.begin(9600);
}

void loop() {
  
  if (Serial.available() > 0) {
    
    String message = Serial.readStringUntil('\n');
    
    if (message.length() > 0) {
      String modifiedMessage = "Modified: " + message;

      
      Serial.println(modifiedMessage);

      
      delay(200);
    }
  }
}
