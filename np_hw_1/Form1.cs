using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace np_hw_1
{
    public partial class Form1 : Form
    {
        int _size = 6;
        List<RoundButton> _buttons;
        bool _isFirstPlayerTurn = true;
        int _quickBallFalling = 30;

        int _firsrtPlayerScore = 0;
        int _secondPlayerScore = 0;

        static int _port = 11000;
        Dictionary<BallColors, Bitmap> _imagesForPlayers;

        public Form1()
        {
            InitializeComponent();

            turn_label.Text = _isFirstPlayerTurn ? "Ход красных" : "Ход синих";
            //this.BackColor = Color.FromArgb(210, 210, 210);

            // images for balls
            _imagesForPlayers = new Dictionary<BallColors, Bitmap>()
            {
                { BallColors.none,    Properties.Resources.none   },
                { BallColors.first,   Properties.Resources.first  },
                { BallColors.second,  Properties.Resources.second }
            };
            // labels for score with images from source
            score_firstPlayer_label.Image = _imagesForPlayers[BallColors.first];
            score_secondPlayer_label.Image = _imagesForPlayers[BallColors.second];


            CreateField();
        }

        // создание поля заданного размера
        private void CreateField()
        {
            this.field_tableLayoutPanel.ColumnCount = _size;
            this.field_tableLayoutPanel.RowCount = _size;

            this.field_tableLayoutPanel.ColumnStyles.Clear();
            float width = this.field_tableLayoutPanel.Width / this.field_tableLayoutPanel.ColumnCount;
            for (int i = 0; i < this.field_tableLayoutPanel.ColumnCount; i++)
            {
                this.field_tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, width));
            }

            this.field_tableLayoutPanel.RowStyles.Clear();
            float height = this.field_tableLayoutPanel.Height / this.field_tableLayoutPanel.ColumnCount;
            for (int i = 0; i < this.field_tableLayoutPanel.RowCount; i++)
            {
                this.field_tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, height));
            }

            _buttons = new List<RoundButton>();
            for (int row = 0; row < _size; row++)
            {
                for (int col = 0; col < _size; col++)
                {
                    int idx = row * _size + col;
                    RoundButton button = new RoundButton
                    {
                        ColorBall = BallColors.none,
                        Image = (Bitmap)_imagesForPlayers[BallColors.none],
                        Width = 85,
                        Height = 85,
                        Tag = idx,
                        //Text = idx.ToString()  // номер шарика
                    };
                    _buttons.Add(button);
                    this.field_tableLayoutPanel.Controls.Add(button, col, row);
                }
            }
        }

        // проверка на выиграш
        private bool IsWin(BallColors color)
        {
            // horizontal check
            int scoreInLine = 0;
            for (int row = 0; row < _size; row++)
            {
                for (int col = 0; col < _size; col++)
                {
                    if (_buttons[row * _size + col].ColorBall == color)
                    {
                        scoreInLine++;
                        if (scoreInLine == 4)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        scoreInLine = 0;
                    }
                }
            }

            // vertical check
            int scoreInColumn = 0;
            for (int col = 0; col < _size; col++)
            {
                for (int row = 0; row < _size; row++)
                {
                    if (_buttons[row * _size + col].ColorBall == color)
                    {
                        scoreInColumn++;
                        if (scoreInColumn == 4)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        scoreInColumn = 0;
                    }
                }
            }

            // diagonal -left_bottom->right-top
            int diagonalScore_1st = 0;
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].ColorBall == color)
                {
                    diagonalScore_1st++;
                    int step = _size - 1;
                    for (int j = i + step; j < _buttons.Count; j += step)
                    {
                        if (_buttons[j].ColorBall == color)
                        {
                            diagonalScore_1st++;
                            if (diagonalScore_1st == 4)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    diagonalScore_1st = 0;
                }
            }

            // diagonal -left_top->right-bottom
            int diagonalScore_2nd = 0;
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i].ColorBall == color)
                {
                    diagonalScore_2nd++;
                    int step = _size + 1;
                    for (int j = i + step; j < _buttons.Count; j += step)
                    {
                        if (_buttons[j].ColorBall == color)
                        {
                            diagonalScore_2nd++;
                            if (diagonalScore_2nd == 4)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    diagonalScore_2nd = 0;
                }
            }
            return false;
        }

        // поиск кнопки в массиве по тэгу
        private RoundButton FindButtonByTag(string tag)
        {
            RoundButton rB = null;
            foreach (RoundButton button in _buttons)
            {
                if (string.Equals(button.Tag.ToString(), tag))
                {
                    rB = button;
                    break;
                }
            }
            return rB;
        }

        bool SocketConnected(Socket s)
        {
            if (s == null)
                return false;

            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }

        private void roundButton_Click(object sender, EventArgs e)
        {
            // блокировка UI во время падения мячика
            if (timer.Enabled)
            {
                return;
            }

            // блокировка поля для сервера, пока клиент не присоединился
            if (_server != null && !SocketConnected(_server.Handler))
            {
                return;
            }

            // блокировка UI, пока не походил другой игрок 
            if (e != null &&
                ((_server != null && !_isFirstPlayerTurn) ||
                (_server == null && _isFirstPlayerTurn)))
            {
                return;
            }

            RoundButton clickedButton = sender as RoundButton;

            // кнопка, на которую будет сделан ход
            // независимо от столбца в котором был нажатие
            _buttonAim = FindAimButton(clickedButton);

            // если возможно сделать ход - игрок ходит
            if (_buttonAim == null)
            {
                return;
            }
            else
            {
                if (_isFirstPlayerTurn)
                {
                    MakePlay(_buttonAim, BallColors.first);
                }
                else
                {
                    MakePlay(_buttonAim, BallColors.second);
                }
                _isFirstPlayerTurn = !_isFirstPlayerTurn;
                turn_label.Text = _isFirstPlayerTurn ? "Ход красных" : "Ход синих";

                if (e == null)
                {
                    return;
                }

                // отправка данных второму игроку
                if (_server != null)
                {
                    byte[] msg = Encoding.UTF8.GetBytes(_buttonAim.Tag.ToString());
                    _server.Handler.Send(msg);
                }
                else
                {
                    byte[] msg = Encoding.UTF8.GetBytes(_buttonAim.Tag.ToString());
                    _client.Socket.Send(msg);
                }
            }
        }

        // поиск кнопки ДО которой летит мячик
        private RoundButton FindAimButton(RoundButton clickedButton)
        {
            RoundButton aim = null;

            if (clickedButton.ColorBall == BallColors.none)
            {
                aim = clickedButton;
            }

            for (int i = (int)clickedButton.Tag; i < _buttons.Count; i += _size)
            {
                RoundButton button = FindButtonByTag(i.ToString());
                if (button.ColorBall == BallColors.none)
                {
                    aim = button;
                }
            }
            return aim;
        }

        // движение конопок при осуществл.хода
        RoundButton _buttonMoved = new RoundButton();   // временная кнопка для движения
        RoundButton _buttonAim = new RoundButton();     // кнопка, на которую нажали
        BallColors _colorTurn;

        private void MakePlay(RoundButton button, BallColors color)
        {
            _colorTurn = color;

            Point point = new Point(button.FindForm().PointToClient(button.Parent.PointToScreen(button.Location)).X,
                                    _buttons[0].FindForm().PointToClient(_buttons[0].Parent.PointToScreen(_buttons[0].Location)).Y);
            // создание временной падающей кнопки
            _buttonMoved = new RoundButton
            {
                ColorBall = color,
                Width = button.Width,
                Height = button.Height,
                Location = point,
            };
            _buttonMoved.Image = _imagesForPlayers[_buttonMoved.ColorBall];
            this.Controls.Add(_buttonMoved);
            _buttonMoved.BringToFront();

            timer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (RoundButton b in _buttons)
            {
                b.Click += roundButton_Click;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Point p = _buttonMoved.Location;
            p.Y += _quickBallFalling;
            _buttonMoved.Location = p;

            this.Invoke((MethodInvoker)delegate
            {
                roundButton_Click(FindButtonByTag(_buttonAim.Tag.ToString()), null);
            });

            if (_buttonMoved.Location.Y > _buttonAim.Location.Y)
            {
                _buttonAim.ColorBall = _colorTurn;
                _buttonAim.Image = _imagesForPlayers[_colorTurn];
                timer.Stop();
                this.Controls.Remove(_buttonMoved);

                if (IsWin(_colorTurn))
                {
                    string colorWin = (string.Equals(_colorTurn.ToString(), "first") ? "Красные" : "Синие") + " выиграли";
                    if (_colorTurn == BallColors.first)
                    {
                        _firsrtPlayerScore++;
                        this.score_firstPlayer_label.Text = _firsrtPlayerScore.ToString();
                    }
                    else
                    {
                        _secondPlayerScore++;
                        this.score_secondPlayer_label.Text = _secondPlayerScore.ToString();
                    }

                    /////////////////////////////////////////////////////////////////////////
                    DialogResult res = MessageBox.Show("Новая игра?", colorWin, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        StartNewGame();
                    }
                    else
                    {
                        // отсоединение
                        this.Close();
                    }
                }
            }
        }

        private void StartNewGame()
        {
            _isFirstPlayerTurn = true;
            turn_label.Text = _isFirstPlayerTurn ? "Ход красных" : "Ход синих";
            foreach (RoundButton button in _buttons)
            {
                button.ColorBall = BallColors.none;
                button.Image = _imagesForPlayers[button.ColorBall];
            }
        }

        Thread _thread;
        Server _server;
        Client _client;

        private void startServer_button_Click(object sender, EventArgs e)
        {
            _server = new Server(_port);
            status_label.Text = _server.Status;
            // status_label.BackColor = Color.Red;
            status_label.Image = _imagesForPlayers[BallColors.first];
            status_label.ForeColor = Color.White;
            ListenGame();

            MessageBox.Show(_server.Ip, "IP-адресс сервера", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void startClient_button_Click(object sender, EventArgs e)
        {
            ClientForm f2 = new ClientForm(_port);
            f2.passControl += f2_passControl;
            f2.ShowDialog();
        }

        void f2_passControl(object sender, EventArgs e)
        {
            try
            {
                _client = new Client(((TextBox)sender).Text, _port);
            }
            catch (Exception)
            {
                MessageBox.Show("Введите IP-адресс сервера");
                return;
            }
            status_label.Text = _client.Status;
            status_label.Image = _imagesForPlayers[BallColors.second];
            //status_label.BackColor = Color.Blue;
            status_label.ForeColor = Color.White;

            ListenGame();
        }

        private void ListenGame()
        {
            if (_server != null)
            {
                _thread = new Thread(() =>
                {
                    while (true)
                    {
                        if (_server.isListening() == false)
                        {
                            break;
                        }
                        this.Invoke((MethodInvoker)delegate
                        {
                            roundButton_Click(FindButtonByTag(_server.Info), null);
                        });
                        Thread.Sleep(10);
                    }
                });
                _thread.Start();
            }
            else
            {
                _thread = new Thread(() =>
                {
                    while (true)
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = _client.Socket.Receive(bytes);
                        string buttonTag = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        this.Invoke((MethodInvoker)delegate
                        {
                            roundButton_Click(FindButtonByTag(buttonTag), null);
                        });
                        Thread.Sleep(10);
                    }
                });
                _thread.Start();
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            byte[] msg = Encoding.ASCII.GetBytes("break");
            if (_server != null)
            {
                if (_thread != null){
                    _thread.Abort();
                }
            }
            else
            {
                if (_client != null)
                {
                    _client.Socket.Send(msg);
                    _client.Socket.Shutdown(SocketShutdown.Both);

                    if (!PingHost(_client.IpEndPoint)){
                        _client.Disconnect();
                    }
                }

                if (_thread != null){
                    _thread.Abort();
                }
            }
        }

        private bool PingHost(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                return false;
            }

            TcpClient client = null;
            try
            {
               client = new TcpClient(endPoint);
               client.Close();
               return true;
            }
            catch (Exception)
            {
                if (client != null) { client.Close(); }
                return false;
            }
        }
    }
}
