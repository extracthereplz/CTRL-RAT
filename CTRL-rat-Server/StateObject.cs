using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CTRL_rat_Server
{
    public class StateObject
    {
        // Size of receive buffer.  
        public const int BufferSize = 4;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public byte[] recived;

        // Client socket.
        public Socket workSocket = null;
    }
}
