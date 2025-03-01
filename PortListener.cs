using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFrag
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    public class PortListener
    {
        private readonly int _port;
        private Socket _socket;

        public PortListener(int port)
        {
            _port = port;
        }

        public bool IsPortInUse()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
                _socket.Listen(1);
                return false;
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    return true;
                }
                throw;
            }
            finally
            {
                _socket?.Close();
            }
        }
    }
}
