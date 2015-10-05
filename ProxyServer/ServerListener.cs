using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;

namespace ProxyServer
{
    class ServerListener
    {
        private int listenPort;
        private TcpListener listener;
        public static ObjectCache cache = MemoryCache.Default;

        public ServerListener(int port)
        {
            foreach (var element in MemoryCache.Default)
            {
                MemoryCache.Default.Remove(element.Key);
            }
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
