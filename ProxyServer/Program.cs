using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerListener simpleHttpProxyServer = new ServerListener(9000);
            simpleHttpProxyServer.StartServer();
            while(true)
            {
                simpleHttpProxyServer.AcceptConnection();
            }
        }
        //public static string data = null;

        //public static void startListening()
        //{
        //    byte[] bytes = new Byte[1024];

        //    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        //    IPAddress ipAdress = ipHostInfo.AddressList[0];
        //    IPEndPoint localEndPoint = new IPEndPoint(ipAdress, 11000);

        //    Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //    try
        //    {
        //        listener.Bind(localEndPoint);
        //        listener.Listen(10);

        //        while(true)
        //        {
        //            Console.WriteLine("Waiting for connection...");

        //            Socket handler = listener.Accept();
        //            data = null;

        //            while(true)
        //            {
        //                bytes = new byte[1024];
        //                int bytesRec = handler.Receive(bytes);
        //                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
        //                if (data.IndexOf("<EOF>") > -1)
        //                {
        //                    break;
        //                }
        //            }
        //            Console.WriteLine("Text received : {0}", data);

        //            byte[] msg = Encoding.ASCII.GetBytes(data);

        //            handler.Send(msg);
        //            handler.Shutdown(SocketShutdown.Both);
        //            handler.Close();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.ToString());
        //    }
        //}

        //public static int Main(String[] args)
        //{
        //    startListening();
        //    return 0;
        //}
    }
}
