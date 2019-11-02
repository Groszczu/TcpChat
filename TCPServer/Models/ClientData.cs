using System.Net.Sockets;

namespace TCPServer.Models
{
    public class ClientData
    {
        public int Id { get; }
        public TcpClient Socket { get; }

        public ClientData(int id, TcpClient socket)
        {
            Id = id;
            Socket = socket;
        }

    }
}
