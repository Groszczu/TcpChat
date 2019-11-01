using System.Net.Sockets;

namespace TCPServer.Models
{
    class ClientData
    {
        public int Id { get; }
        public int SessionId { get; }
        public TcpClient Socket { get; }

        public ClientData(int id, int sessionId, TcpClient socket)
        {
            Id = id;
            SessionId = sessionId;
            Socket = socket;
        }

    }
}
