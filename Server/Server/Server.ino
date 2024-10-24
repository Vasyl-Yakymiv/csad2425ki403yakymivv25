#include <stdlib.h>
#include <time.h>  
#include <limits.h>
char board[3][3]; // Поле для хрестиків-нуликів
char currentPlayer = 'X'; // Поточний гравець

void setup() {
  Serial.begin(9600); // Налаштування серійного порту
  resetBoard(); // Очищення поля для нової гри
}

void loop() {
  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n'); // Отримуємо команду від клієнта
    processCommand(command);
  }
}

// Очищення поля гри
void resetBoard() {
  for (int i = 0; i < 3; i++) {
    for (int j = 0; j < 3; j++) {
      board[i][j] = ' ';
    }
  }
  currentPlayer = 'X'; // Перший хід за Х
}

void processCommand(String command) {
    if (command.startsWith("MOVE")) {
        int row = command.charAt(5) - '0'; // Рядок (0-2)
        int col = command.charAt(7) - '0'; // Колонка (0-2)
     
        if (board[row][col] == ' ') { // Якщо клітинка порожня
            board[row][col] = currentPlayer; // Зробити хід
            Serial.println("OK:MOVE"); // Відправити відповідь клієнту

            char winner = checkWin(); // Отримуємо переможця
            if (winner != ' ') { // Якщо є переможець
                Serial.print("WIN:");
                Serial.println(winner); // Відправити переможця ('X' або 'O')
                resetBoard(); // Скинути поле
                winner = ' ';
            } else if (isBoardFull()) {
        Serial.println("DRAW"); // Нічия
        resetBoard();
        resetBoard();
            } else {
                currentPlayer = (currentPlayer == 'X') ? 'O' : 'X'; // Зміна гравця
            }
        }
    }
    if (command == "AI_MOVE") {
        makeAIMove();
    }
    if (command == "AI_MOVE_WIN") {
        makeAIMoveWin();
            }
     else if (command == "NEW") {
        resetBoard();
        Serial.println("OK:NEW_GAME");
    } else if (command == "CLEAR_BOARD") {
        resetBoard(); // Очищення поля
        Serial.println("OK:CLEAR_BOARD"); // Підтвердження очищення
    }
}

char checkWin() {
  for (int i = 0; i < 3; i++) {
    if (board[i][0] != ' ' && board[i][0] == 'X' && board[i][0] == board[i][1] && board[i][1] == board[i][2]) {
      return 'X'; // Повертаємо переможця ('X' або 'O')
    }
  }
  for (int i = 0; i < 3; i++) {
    if (board[i][0] != ' ' && board[i][0] == 'O' && board[i][0] == board[i][1] && board[i][1] == board[i][2]) {
      return 'O'; // Повертаємо переможця ('X' або 'O')
    }
  }
  // Перевірка колонок
  for (int i = 0; i < 3; i++) {
    if (board[0][i] != ' ' && board[0][i] == 'X' && board[0][i] == board[1][i] && board[1][i] == board[2][i]) {
      return 'X'; // Повертаємо переможця ('X' або 'O')
    }
  }
  for (int i = 0; i < 3; i++) {
    if (board[0][i] != ' ' && board[0][i] == 'O' && board[0][i] == board[1][i] && board[1][i] == board[2][i]) {
      return 'O'; // Повертаємо переможця ('X' або 'O')
    }
  }
  if (board[0][0] != ' ' && board[0][0] == 'X' && board[0][0] == board[1][1] && board[1][1] == board[2][2]) {
    return 'X'; // Повертаємо переможця ('X' або 'O')
  }
  if (board[0][2] != ' ' && board[0][2] == 'X' && board[0][2] == board[1][1] && board[1][1] == board[2][0]) {
    return 'X'; // Повертаємо переможця ('X' або 'O')
  }
   if (board[0][0] != ' ' && board[0][0] == 'O' && board[0][0] == board[1][1] && board[1][1] == board[2][2]) {
    return 'O'; // Повертаємо переможця ('X' або 'O')
  }
  if (board[0][2] != ' ' && board[0][2] == 'O' && board[0][2] == board[1][1] && board[1][1] == board[2][0]) {
    return 'O'; // Повертаємо переможця ('X' або 'O')
  }
  return ' '; // Немає переможця
}
bool isBoardFull() {
  for (int i = 0; i < 3; i++) {
    for (int j = 0; j < 3; j++) {
      if (board[i][j] == ' ') {
        return false;
      }
    }
  }
  return true;
}

void makeAIMove() {
    // Генерація випадкових чисел для рядка і стовпця
    int row, col;
    bool moveMade = false;
    srand(time(NULL));
    // Пробуємо зробити хід до тих пір, поки не знайдемо вільну клітинку
    for (int attempts = 0; attempts < 10; attempts++) { // Максимум 10 спроб
        row = rand() % 3; // Випадковий рядок (0, 1, 2)
        col = rand() % 3; // Випадковий стовпець (0, 1, 2)

        if (board[row][col] == ' ') { // Якщо клітинка порожня
            board[row][col] = 'O'; 

            Serial.print("AI_MOVE:");
            Serial.print(row);
            Serial.print(",");
            Serial.println(col);

            // Перевірка на переможця після ходу AI
            char winner = checkWin(); 
            if (winner != ' ') { // Якщо є переможець
                Serial.print("WIN:");
                Serial.println(winner); // Відправити переможця ('X' або 'O')
                resetBoard(); // Скинути поле
            } else if (isBoardFull()) {
                Serial.println("DRAW"); // Нічия
                resetBoard();
            } else {
                currentPlayer = 'X'; // Зміна гравця на 'X' після ходу AI
            }

            moveMade = true; // Хід успішно виконано
            break; // Вийти з циклу, оскільки хід зроблено
        }
    }

}
void makeAIMoveWin() {
    int row = -1, col = -1;

    // Перевірка на перший хід
    if (isFirstMove()) {
        row = 1; // Центр
        col = 1;
    } 
    else if (findWinningMove('X', row, col)) {
        // Якщо AI може виграти, виконуємо хід
    } 
    else if (findWinningMove('O', row, col)) {
        // Блокуємо можливість виграшу супротивника
    } 
    else {
        // Випадковий хід, якщо немає інших варіантів
        for (int attempts = 0; attempts < 9; attempts++) { // Максимум 9 спроб
            row = rand() % 3; // Випадковий рядок (0, 1, 2)
            col = rand() % 3; // Випадковий стовпець (0, 1, 2)
            if (board[row][col] == ' ') { // Якщо клітинка порожня
                break; // Зупиняємо цикл, якщо знайшли вільну клітинку
            }
        }
    }

    // Здійснюємо хід, якщо знайдено допустиме місце
    if (row != -1 && col != -1) {
        board[row][col] = 'X'; // Зробити хід X
    }

    // Відправка ходів до клієнта
    Serial.print("AI_MOVE_WIN:");
    Serial.print(row);
    Serial.print(",");
    Serial.println(col);

    // Перевірка на повноту дошки
    if (isBoardFull()) {
        Serial.println("DRAW"); // Нічия
        resetBoard();
    }
    
    // Зміна гравця
    currentPlayer = (currentPlayer == 'X') ? 'O' : 'X'; 
}

// Функція для перевірки на виграшний хід
bool findWinningMove(char player, int &row, int &col) {
    char opponent = (player == 'X') ? 'O' : 'X';

    // Перевірка рядків, стовпців та діагоналей
    for (int i = 0; i < 3; i++) {
        // Для рядків
        if ((board[i][0] == player && board[i][1] == player && board[i][2] == ' ')) { row = i; col = 2; return true; }
        if ((board[i][0] == player && board[i][2] == player && board[i][1] == ' ')) { row = i; col = 1; return true; }
        if ((board[i][1] == player && board[i][2] == player && board[i][0] == ' ')) { row = i; col = 0; return true; }

        // Для стовпців
        if ((board[0][i] == player && board[1][i] == player && board[2][i] == ' ')) { row = 2; col = i; return true; }
        if ((board[0][i] == player && board[2][i] == player && board[1][i] == ' ')) { row = 1; col = i; return true; }
        if ((board[1][i] == player && board[2][i] == player && board[0][i] == ' ')) { row = 0; col = i; return true; }
    }

    // Перевірка діагоналей
    if ((board[0][0] == player && board[1][1] == player && board[2][2] == ' ')) { row = 2; col = 2; return true; }
    if ((board[0][2] == player && board[1][1] == player && board[2][0] == ' ')) { row = 2; col = 0; return true; }
    
    // Якщо не знайшли виграшний хід, повертаємо false
    return false;
}

// Функція для перевірки, чи це перший хід
bool isFirstMove() {
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            if (board[i][j] != ' ') {
                return false; // Є вже зроблені ходи
            }
        }
    }
    return true; // Поле порожнє, перший хід
}

