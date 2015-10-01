using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ProxyServer
{
    class ClientConnection
    {
        private Socket clientSocket;

        public ClientConnection(Socket client)
        {
            this.clientSocket = client;
        }

        public void StartHandling()
        {
            Thread handler = new Thread(Handler);
            handler.Priority = ThreadPriority.AboveNormal;
            handler.Start();
        }

        private void Handler()
        {
            bool recvRequest = true;
            string EOL = "\r\n";

            string requestPayload = "";
            string requestTempLine = "";
            List<string> requestLines = new List<string>();
            byte[] requestBuffer = new byte[1];
            byte[] responseBuffer = new byte[1];

            requestLines.Clear();

            try
            {
                while(recvRequest)
                {
                    this.clientSocket.Receive(requestBuffer);
                    string fromByte = ASCIIEncoding.ASCII.GetString(requestBuffer);
                    requestPayload += fromByte;
                    requestTempLine += fromByte;

                    if(requestTempLine.EndsWith(EOL))
                    {
                        requestLines.Add(requestTempLine.Trim());
                        requestTempLine = "";
                    }
                    if(requestPayload.EndsWith(EOL + EOL))
                    {
                        recvRequest = false;
                    }
                }
                Console.WriteLine("Raw Request Received...");
                Console.WriteLine(requestPayload);

                string remoteHost = requestLines[0].Split(' ')[1].Replace("http://", "").Split('/')[0];
                string requestFile = requestLines[0].Replace("http://", "").Replace(remoteHost, "");
                //Console.WriteLine(requestFile + ":1");
                requestLines[0] = requestFile;

                requestPayload = "";
                foreach(string line in requestLines)
                {
                    requestPayload += line;
                    requestPayload += EOL;
                }

                Console.WriteLine(remoteHost + ":2");
                if(remoteHost.Contains(':'))
                {
                    remoteHost = remoteHost.Remove(remoteHost.IndexOf(':'));
                }
                IPAddress[] ips = Dns.GetHostAddresses(remoteHost);
                Console.WriteLine(ips[0].ToString() + ":3");

                Socket destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                destServerSocket.Connect(ips[0].ToString(), 80);

                destServerSocket.Send(ASCIIEncoding.ASCII.GetBytes(requestPayload));

                while(destServerSocket.Receive(responseBuffer) != 0)
                {
                    this.clientSocket.Send(responseBuffer);
                }

                destServerSocket.Disconnect(false);
                destServerSocket.Dispose();
                this.clientSocket.Disconnect(false);
                this.clientSocket.Dispose();
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Occurred: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
