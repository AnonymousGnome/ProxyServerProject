using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Caching;
using System.IO;
using System.Web;

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
            //ObjectCache cache = MemoryCache.Default;

            bool recvRequest = true;
            string EOL = "\r\n";

            string requestPayload = "";
            string requestTempLine = "";
            string logLineItem = System.DateTime.Now.ToString("MMM dd yyyy hh:mm:ss ");
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
                //Console.WriteLine(requestPayload);
                
                string remoteHost = requestLines[0].Split(' ')[1].Replace("http://", "").Split('/')[0];
                logLineItem += requestLines[0].Split(' ')[1] + " ";
                string requestFile = requestLines[0].Replace("http://", "").Replace(remoteHost, "");
                //Console.WriteLine(remoteHost + ":1");
                requestLines[0] = requestFile;

                requestPayload = "";
                foreach(string line in requestLines)
                {
                    requestPayload += line;
                    requestPayload += EOL;
                    if(line.Contains("User-Agent"))
                        logLineItem += line.Split(' ')[10].Split('/')[1];
                }

                var cachedRequest = ServerListener.cache[requestFile] as List<byte[]>;

                if (cachedRequest == null)
                {
                    CacheItemPolicy policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(60.0);

                    //List<string> filePaths = new List<string>();
                    //string cachedFilePath = @"E:\CSCI 415\Assignment 2\ProxyServer\ProxyServer\bin\Debug\cacheText.txt";
                    //filePaths.Add(cachedFilePath);

                    //policy.ChangeMonitors.Add(new HostFileChangeMonitor(filePaths));

                    IPAddress[] ips = Dns.GetHostAddresses(remoteHost.Split(':')[0]);

                    Socket destServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    if (remoteHost.Contains(':'))
                    {
                        destServerSocket.Connect(ips[0], Convert.ToInt32(remoteHost.Split(':')[1]));
                        if (destServerSocket.Connected)
                        {
                            Console.WriteLine("Connection established...");
                        }
                    }
                    else
                    {
                        destServerSocket.Connect(ips[0], 80);
                        if (destServerSocket.Connected)
                        {
                            Console.WriteLine("Connection established on port 80...");
                        }
                    }

                    //else
                    //{
                    //    //Console.WriteLine("Failure establishing connection...");
                    //}

                    //Console.WriteLine("Sending Request...");
                    //Console.WriteLine("5:" + requestPayload + ":5");
                    if(destServerSocket.Connected)
                        destServerSocket.Send(ASCIIEncoding.ASCII.GetBytes(requestPayload));

                    List<string> responseLines = new List<string>();
                    List<byte[]> responseBytes = new List<byte[]>();
                    string responseTempLine = "";
                    while (destServerSocket.Receive(responseBuffer) != 0)
                    {
                        //Console.Write(ASCIIEncoding.ASCII.GetString(responseBuffer));
                        this.clientSocket.Send(responseBuffer);
                        responseBytes.Add(responseBuffer);
                        responseTempLine += ASCIIEncoding.ASCII.GetString(responseBuffer);

                        if (responseTempLine.EndsWith(EOL))
                        {
                            responseLines.Add(responseTempLine.Trim());
                            responseTempLine = "";
                        }
                    }

                    foreach (string line in responseLines)
                    {
                        //Console.WriteLine(line);
                        if (line.Contains("Content-Length"))
                        {
                            //Console.WriteLine(line);
                            logLineItem += line.Split(':')[1];
                        }
                    }

                    if(responseBytes.Count != 0)
                        ServerListener.cache.Set(requestFile, responseBytes, policy);

                    //destServerSocket.Disconnect(false);
                    destServerSocket.Dispose();
                }
                else
                {
                    Console.WriteLine(cachedRequest.Count);
                    Console.WriteLine("Retrieving from cache...");
                    for (int i = 0; i < cachedRequest.Count; i++)
                    {
                        clientSocket.Send(cachedRequest.ElementAt(i));
                    }
                }
                //Console.WriteLine(":6");
                File.AppendAllText(@".\proxy.log", logLineItem + EOL);

                //this.clientSocket.Disconnect(false);
                this.clientSocket.Dispose();
            }
            catch(Exception e)
            {
                //Console.WriteLine("Error Occurred: " + e.Message);
                //Console.WriteLine(e.StackTrace);
            }
        }
    }
}
