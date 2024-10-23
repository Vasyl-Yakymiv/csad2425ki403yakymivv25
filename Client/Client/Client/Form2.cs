using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Client
{
    public partial class Form2 : Form
    {
       
        private string currentPlayer = "X";
        private SerialPort arduinoPort;
        private int playerXScore = 0; 
        private int playerOScore = 0; 

        public Form2(SerialPort port)
        {
            InitializeComponent();
            arduinoPort = port;
           
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (arduinoPort.IsOpen)
            {
                arduinoPort.WriteLine("CLEAR_BOARD");
                arduinoPort.Close();
            }
        }

        private void New_Game_Click_1(object sender, EventArgs e)
        {
            arduinoPort.WriteLine("NEW");
            string response = arduinoPort.ReadLine();
            if (response.Contains("OK:NEW_GAME"))
            {
                ResetBoardUI();
            }
            playerXScore = 0;
            playerOScore = 0;
            UpdateScoreLabels();

        }
        private void ResetBoardUI()
        {

            foreach (Button btn in this.Controls.OfType<Button>())
            {
                if (btn.Tag != null)
                {
                    btn.Text = "";
                    btn.Enabled = true; // Активуємо кнопку
                }
            }
        }

        private void Save_Game_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML файли (*.xml)|*.xml";
            saveFileDialog.Title = "Збереження гри";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;

                XElement gameConfig = new XElement("GameConfig",
                    new XElement("Mode", "ManVsMan"),
                    new XElement("Scores",
                        new XElement("PlayerXScore", playerXScore),
                        new XElement("PlayerOScore", playerOScore)
                    )
                );

                gameConfig.Save(filePath);
                MessageBox.Show("Гру збережено!");
            }
        }
        private void Load_Game_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML файли (*.xml)|*.xml";
            openFileDialog.Title = "Завантаження гри";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                XDocument doc = XDocument.Load(filePath);
                XElement gameConfig = doc.Element("GameConfig");

                playerXScore = int.Parse(gameConfig.Element("Scores").Element("PlayerXScore").Value);
                playerOScore = int.Parse(gameConfig.Element("Scores").Element("PlayerOScore").Value);

                label10.Text = $"{playerXScore}";
                label11.Text = $"{playerOScore}";

                MessageBox.Show("Гру завантажено! Рахунок: X - " + playerXScore + ", O - " + playerOScore);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int row = Convert.ToInt32(btn.Tag.ToString()[0].ToString());
            int col = Convert.ToInt32(btn.Tag.ToString()[1].ToString());

            if (currentPlayer == "X")
            {
                string command = $"MOVE {row},{col}";
                arduinoPort.WriteLine(command);

                Task.Run(() =>
                {
                    string response = arduinoPort.ReadLine();
                    ProcessResponse(response, btn);
                });
            }
        }
        private void AI_Move_Click(object sender, EventArgs e)
        {
            if (currentPlayer == "O") // AI грає за "O"
            {
                string command = "AI_MOVE"; // Відправляємо команду AI_MOVE на сервер
                arduinoPort.WriteLine(command);

                // Асинхронно очікуємо відповідь від сервера
                Task.Run(() =>
                {
                    string response = arduinoPort.ReadLine(); // Читаємо відповідь
                    Invoke((MethodInvoker)delegate
                    {
                        ProcessResponse(response, null); // Обробляємо відповідь з AI ходу
                    });
                });
            }
        }
        private void ProcessResponse(string response, Button btn)
        {
            if (response.StartsWith("AI_MOVE"))
            {
                string[] parts = response.Split(':')[1].Split(','); // Отримуємо координати AI ходу
                int row = int.Parse(parts[0]);
                int col = int.Parse(parts[1]);

                // Отримуємо кнопку за координатами
                Button aiButton = GetButtonByCoords(row, col);
                if (aiButton != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        aiButton.Text = "O"; // AI грає за O
                        aiButton.Enabled = false; // Вимикаємо кнопку, щоб не можна було зробити на неї повторний хід
                        currentPlayer = "X"; // Тепер хід гравця
                    });
                }
            }
            if (response.Contains("OK:MOVE"))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    btn.Text = currentPlayer;
                    btn.Enabled = false;
                    currentPlayer = (currentPlayer == "X") ? "O" : "X";
                });
            }
            else if (response.Contains("ERROR:ALREADY_FILLED"))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Ця клітинка вже заповнена!");
                });
            }
            else if (response.StartsWith("DRAW"))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Нічия");
                    arduinoPort.WriteLine("NEW");
                    ClearBoard();
                    btn.Enabled = true;
                    currentPlayer = "X";
                });
            }
            else if (response.StartsWith("WIN:"))
            {
                string winner = response.Substring(4);
                this.Invoke((MethodInvoker)delegate
                {
                    if (winner == "X\r")
                    {
                        playerXScore++;
                        MessageBox.Show($"Гравець 1 виграв!");
                        UpdateScoreLabels();
                        ResetBoardUI();
                        arduinoPort.WriteLine("NEW");
                        ClearBoard();
                        currentPlayer = "X";
                    }
                    else if (winner == "O\r")
                    {
                        playerOScore++;
                        MessageBox.Show($"Гравець 2 виграв!");
                        UpdateScoreLabels();
                        ResetBoardUI();
                        arduinoPort.WriteLine("NEW");
                        ClearBoard();
                        currentPlayer = "X";
                    }
                });
            }
        }
        private void UpdateScoreLabels()
        {
            label10.Text = $"{playerXScore}";
            label11.Text = $"{playerOScore}";
        }

        private Button GetButtonByCoords(int row, int col)
        {
            var buttonsWithTag = this.Controls.OfType<Button>().Where(btn => btn.Tag != null);

            foreach (Button btn in buttonsWithTag)
            {
                if (btn.Tag.ToString() == $"{row}{col}")
                {
                    return btn;
                }
            }
            return null;
        }

        private void ClearBoard()
        {
            this.Invoke((MethodInvoker)delegate
            {
                button1.Text = " ";
                button2.Text = " ";
                button3.Text = " ";
                button4.Text = " ";
                button5.Text = " ";
                button6.Text = " ";
                button7.Text = " ";
                button8.Text = " ";
                button9.Text = " ";
            });
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            button1.TabStop = false;
            button2.TabStop = false;
            button3.TabStop = false;
            button4.TabStop = false;
            button5.TabStop = false;
            button6.TabStop = false;
            button7.TabStop = false;
            button8.TabStop = false;
            button9.TabStop = false;
            button10.TabStop = false;
            New_Game.TabStop = false;
            Save_Game.TabStop = false;
            Load_Game.TabStop = false;
            arduinoPort.WriteLine("NEW");
            ClearBoard();
            currentPlayer = "X";
            button1.Tag = "00";
            button2.Tag = "01";
            button3.Tag = "02";
            button4.Tag = "10";
            button5.Tag = "11";
            button6.Tag = "12";
            button7.Tag = "20";
            button8.Tag = "21";
            button9.Tag = "22";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            arduinoPort.WriteLine("NEW");
            string response = arduinoPort.ReadLine();
            if (response.Contains("OK:NEW_GAME"))
            {
                ResetBoardUI();
            }
            currentPlayer = "X";
        }

       
    }
}
