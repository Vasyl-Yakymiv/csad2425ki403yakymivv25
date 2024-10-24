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
    public partial class Form3 : Form
    {
        private string currentPlayer = "X";
        private SerialPort arduinoPort;
        private int playerXScore = 0;
        private int playerOScore = 0;
        public Form3(SerialPort port)
        {
            InitializeComponent();
            arduinoPort = port;

        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (arduinoPort.IsOpen)
            {
                arduinoPort.WriteLine("CLEAR_BOARD");
                arduinoPort.Close();
            }
        }

        private void Form3_Load(object sender, EventArgs e)
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

        private void New_Game_Click(object sender, EventArgs e)
        {
            arduinoPort.WriteLine("NEW");
            string response = arduinoPort.ReadLine();
            if (response.Contains("OK:NEW_GAME"))
            {
                ResetBoardUI();
            }
           
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

        private void Save_Game_Click(object sender, EventArgs e)
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

        private void Load_Game_Click(object sender, EventArgs e)
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

                

                MessageBox.Show("Гру завантажено! Рахунок: X - " + playerXScore + ", O - " + playerOScore);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void AI_Move_Click_1(object sender, EventArgs e)
        {
                string command = "AI_MOVE_WIN";
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
        private void ProcessResponse(string response, Button btn)
        {
            if (response.StartsWith("AI_MOVE_WIN"))
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
                        aiButton.Text = currentPlayer;
                        aiButton.Enabled = false; // Вимикаємо кнопку, щоб не можна було зробити на неї повторний хід
                        currentPlayer = (currentPlayer == "X") ? "O" : "X";
                    });
                }
            }
            
            if (response.Contains("ERROR:ALREADY_FILLED"))
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Ця клітинка вже заповнена!");
                });
            }
            
           
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
