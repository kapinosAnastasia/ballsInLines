using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace np_hw_1
{
    class Client
    {
        string _status = "Клиент";
        Socket _socket;
        IPEndPoint _ipEndPoint;

        public Client(string ip, int port)
        {
          IPAddress  ipA = IPAddress.Parse(ip);

            _ipEndPoint = new IPEndPoint(ipA, port);
            _socket = new Socket(ipA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Соединяем сокет с удаленной точкой
            _socket.Connect(_ipEndPoint);
        }

        public IPEndPoint IpEndPoint
        {
            get { return _ipEndPoint; }
        }

        public string Status
        {
            get { return this._status; }
        }

        public Socket Socket
        {
            get { return _socket; }
        }

        public void Disconnect()
        {
           if (_socket != null) _socket.Close();
        }
    }
}
