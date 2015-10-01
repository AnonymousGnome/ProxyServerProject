using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer
{
    class ServerListener
    {
        private int listenPort;
        private TcpListener listener;

        public ServerListener(int port)
        {
            this.listenPort = port;
            this.listener = new TcpListener(IPAddress.Any, this.listenPort);
        }

        public void StartServer()
        {
            this.listener.Start();
        }

        public void AcceptConnection()
        {
            Socket newClient = this.listener.AcceptSocket();
            ClientConnection client = new ClientConnection(newClient);
            client.StartHandling();
        }
    }
}
