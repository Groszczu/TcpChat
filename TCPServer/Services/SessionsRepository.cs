using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TCPServer.Models;

namespace TCPServer.Services
{
    public class SessionsRepository: ISessionsRepository
    {
        private Dictionary<ClientData, Guid> _sessions;
        public void AddSession(ClientData clientData, Guid sessionId)
        {
            _sessions.Add(clientData, sessionId);
        }

        public Guid GetSessionId(ClientData clientData)
        {
            if (!_sessions.ContainsKey(clientData))
                throw new InvalidOperationException("Required client doesn't belong to any active session");

            return _sessions[clientData];
        }
    }
}
