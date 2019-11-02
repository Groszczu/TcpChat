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
        private readonly Dictionary<ClientData, Guid> _sessions = new Dictionary<ClientData, Guid>();
        public void AddSession(ClientData clientData, Guid sessionId)
        {
            try
            {
                _sessions.Add(clientData, sessionId);

            }
            catch (ArgumentException)
            {
                Console.WriteLine("Client you are trying to add is already in the session repository");
            }
        }

        public Guid GetSessionId(ClientData clientData)
        {
            if (!_sessions.ContainsKey(clientData))
                throw new InvalidOperationException("Required client doesn't belong to any active session");

            return _sessions[clientData];
        }

        public Dictionary<ClientData, Guid>.KeyCollection GetAllClients()
        {
            return _sessions.Keys;
        }
    }
}
