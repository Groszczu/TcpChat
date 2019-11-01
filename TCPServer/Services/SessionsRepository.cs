using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPServer.Services
{
    public class SessionsRepository: ISessionsRepository
    {
        public Task AddSession(Guid sessionId, NetworkStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
