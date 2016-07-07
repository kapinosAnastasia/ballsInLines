using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace np_hw_1
{
    class Server
    {
        string _status = "Сервер";
        string _ip;
        Socket _socket;
        Socket _handler;
        IPEndPoint _ipEndPoint;

        public Server(int port)
        {
            String host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[1];
            foreach (var ip in ipHost.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _ip = ip.ToString();
                    break;
                }
            }
           _ipEndPoint = new IPEndPoint(ipAddr, port);

            try
            {
                // Создаем сокет Tcp/Ip
                _socket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(_ipEndPoint);
                _socket.Listen(10);
            }
            catch (System.Net.Sockets.SocketException)
            {

            }
        }

        public IPEndPoint IpEndPoint
        {
            get { return this._ipEndPoint; }
        }

        public string Status
        {
            get { return this._status; }
        }

        public string Ip
        {
            get { return this._ip; }
        }

        public Socket Handler
        {
            get { return _handler; }
        }

        public Socket Socket
        {
            get { return _socket; }
        }

        public string Info;
        
        // слушаем
        public bool isListening()
        {
            // Программа приостанавливается, ожидая входящее соединение
            if (_handler == null)
            {
                _handler = _socket.Accept();
            }

            // Мы дождались клиента, пытающегося с нами соединиться
            byte[] bytes = new byte[1024];
            int bytesRec = _handler.Receive(bytes);

            string button = null;
            button += Encoding.UTF8.GetString(bytes, 0, bytesRec);
            Info = button;

            //!!!!проверка на закрытие
            if (button.Equals("break"))
            {
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (_socket != null)    _socket.Close();
            if (_handler != null)   _handler.Close();
        }
    }
}
