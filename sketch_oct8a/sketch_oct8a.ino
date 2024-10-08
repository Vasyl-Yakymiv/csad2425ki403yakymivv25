void setup() {
  Serial.begin(9600);
}

void loop() {
  // Перевіряємо наявність вхідних даних
  if (Serial.available() > 0) {
    // Читаємо вхідне повідомлення з завершенням рядка
    String message = Serial.readStringUntil('\n');
    
    // Якщо повідомлення не порожнє, модифікуємо його
    if (message.length() > 0) {
      String modifiedMessage = "Modified: " + message;

      // Відправляємо модифіковане повідомлення назад
      Serial.println(modifiedMessage);

      // Додаємо затримку для стабільності
      delay(200);  // Збільшили затримку на Arduino для обробки
    }
  }
}
