using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CTRL_rat_Server;
namespace CTRL_rat_Server
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //make this non-blocking, dont forget to add the Onconnect action (Lisener.onconnect or something like that)
            //onconnect make it send the request info data, on the opcode something like "clientinfo", add that data to listview
            //work on worker sockets
            //var listen = new Listener();/
            //listen.Onconnect = handlers.Onconnect;/
            //listen.StartListening(11000);/
            //Listener.StartListening(11000);
            Application.EnableVisualStyles();
            
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
