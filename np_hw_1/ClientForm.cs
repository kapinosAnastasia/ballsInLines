using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace np_hw_1
{
    public partial class ClientForm : Form
    {
        public delegate void PassControl(object sender, EventArgs e);
        public event PassControl passControl;

        IPEndPoint _ipEndPoint;
        int _port;

        public ClientForm(int port)
        {
            InitializeComponent();
            this._port = port;

            this.ActiveControl = textBox_ip;
        }

        private void ok_button_Click(object sender, EventArgs e)
        {
            // check - is it possiple to connect
            try
            {
                IPAddress ipAddress = IPAddress.Parse(textBox_ip.Text);

                this._ipEndPoint = new IPEndPoint(ipAddress, _port);
            }
            catch (Exception)
            {
                if (!PingHost(_ipEndPoint))
                {
                    MessageBox.Show("Введите IP-сервера.");
                    return;
                }
            }

            if (passControl != null)
            {
                passControl(textBox_ip, null);
            }
            this.Close();
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

        private void textBox_ip_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ok_button_Click(null, null);
            }
        }
    }
}
