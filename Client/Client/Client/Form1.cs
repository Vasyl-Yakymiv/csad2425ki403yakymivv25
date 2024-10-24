using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Xml.Linq;
using System.Threading;

namespace Client
{
    public partial class Form1 : Form
    {
        SerialPort arduinoPort;
        private int playerXScore = 0; // Рахунок гравця X
        private int playerOScore = 0; // Рахунок гравця O
        private string currentPlayer = "X"; // Початковий гравець
        private string gameMode = "ManVsMan";
        public Form1()
        {
            InitializeComponent();
            arduinoPort = new SerialPort("COM6", 9600); // Вказати ваш порт
            arduinoPort.Open(); // Відкрити порт для зв'язку

            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Відправляємо команду для очищення поля на сервер
            if (arduinoPort.IsOpen)
            {
                arduinoPort.WriteLine("CLEAR_BOARD");
                string response = arduinoPort.ReadLine();
                if (response.Contains("OK:CLEAR_BOARD"));
            }
        }

        private void New_Game_Click(object sender, EventArgs e)
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
                    btn.Enabled = true;
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

                label10.Text = $"{playerXScore}";
                label11.Text = $"{playerOScore}";

                MessageBox.Show("Гру завантажено! Рахунок: X - " + playerXScore + ", O - " + playerOScore);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int row = Convert.ToInt32(btn.Tag.ToString()[0].ToString());
            int col = Convert.ToInt32(btn.Tag.ToString()[1].ToString());

            string command = $"MOVE {row},{col}";
            arduinoPort.WriteLine(command);


            Task.Run(() =>
            {
                string response = arduinoPort.ReadLine();
                ProcessResponse(response, btn);
            });
        }

        private void ProcessResponse(string response, Button btn)
        {
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
                    MessageBox.Show($"Нічия!");
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
                    ClearBoard();
                    currentPlayer = "X";
                }
                else if (winner == "O\r")
                    {
                        playerOScore++;
                        MessageBox.Show($"Гравець 2 виграв!");
                        UpdateScoreLabels();
                        ResetBoardUI();
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


        private void Form1_Load(object sender, EventArgs e)
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
            button11.TabStop = false;
            button12.TabStop = false;
            New_Game.TabStop = false;
            Save_Game.TabStop = false;
            Load_Game.TabStop = false;

            arduinoPort.WriteLine("NEW");
            ResetBoardUI();
            ClearBoard();
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
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                Form2 form2 = new Form2(arduinoPort); // Передаємо об'єкт порту
                form2.Show();

            }
            else
            {
                MessageBox.Show("Порт COM6 не відкритий.");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            arduinoPort.WriteLine("NEW");
            string response = arduinoPort.ReadLine();
            if (response.Contains("OK:NEW_GAME"))
            {
                ResetBoardUI();
            }
            currentPlayer = "X";
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (arduinoPort != null && arduinoPort.IsOpen)
            {
                Form3 form3 = new Form3(arduinoPort); // Передаємо об'єкт порту
                form3.Show();

            }
            else
            {
                MessageBox.Show("Порт COM6 не відкритий.");
            }
        }
    }

          
}

