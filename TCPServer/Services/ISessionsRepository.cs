using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.Services
{
    public interface ISessionsRepository
    {
        Task AddSession(Guid sessionId, NetworkStream stream);
    }
}
