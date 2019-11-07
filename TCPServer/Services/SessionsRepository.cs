using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPServer.Models;

namespace TCPServer.Services
{
    public class SessionsRepository : ISessionsRepository
    {
        private readonly Dictionary<ClientData, Guid> _sessions = new Dictionary<ClientData, Guid>();

        public void AddSessionRecord(ClientData clientData, Guid sessionId)
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

        public void UpdateClientSessionId(ClientData clientData, Guid newSessionId)
        {
            if (!_sessions.ContainsKey(clientData))
                throw new ArgumentException(
                    "Cannot update session ID of client that is not in the sessions repository");
            _sessions[clientData] = newSessionId;
        }

        public ClientData GetSecondClientFromSession(Guid sessionId, ClientData referenceClient)
        {
            var sameSessionIdPairs = _sessions.Where(kv => kv.Value == sessionId);
            var secondClientInSession = sameSessionIdPairs.FirstOrDefault(pair => pair.Key != referenceClient).Key;
            return secondClientInSession;
        }

        public string GetOtherClientsIdsToString(ClientData referenceClient)
        {
            var result = new StringBuilder();
            var otherClients = _sessions.Keys.Where(client => client.Id != referenceClient.Id).ToArray();

            if (otherClients.Length == 0)
            {
                result.Append("No other clients connected to server");
            }
            else
            {
                result.Append("Other client IDs: ");

                var delimiter = string.Empty;
                foreach (var clientData in otherClients)
                {
                    result.Append(delimiter + clientData.Id);
                    delimiter = ", ";
                }
            }

            return result.ToString();
        }

        public int GetNumberOfClientsInSession(Guid sessionId)
        {
            return _sessions.Count(kv => kv.Value == sessionId);
        }

        public void RemoveClient(ClientData client)
        {
            _sessions.Remove(client);
            client.ToClose = true;
        }

        public Guid GetSessionId(ClientData clientData)
        {
            if (!_sessions.ContainsKey(clientData))
                throw new InvalidOperationException("Required client doesn't belong to any active session");

            return _sessions[clientData];
        }

        public ClientData GetClientById(int id)
        {
            var searched = _sessions.Keys.FirstOrDefault(client => client.Id == id);
            if (searched == null)
                throw new InvalidOperationException("Required client is not in the sessions repository");

            return searched;
        }

        public bool IsSessionFull(Guid sessionId)
        {
            return GetNumberOfClientsInSession(sessionId) == 2;
        }

        public bool IsEmpty()
        {
            return _sessions.Count == 0;
        }
    }
}