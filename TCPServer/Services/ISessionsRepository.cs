using System;
using TCPServer.Models;

namespace TCPServer.Services
{
    public interface ISessionsRepository
    {
        void AddSessionRecord(ClientData clientData, Guid sessionId);
        Guid GetSessionId(ClientData clientData);
        ClientData GetClientById(int id);
        void UpdateClientSessionId(ClientData clientData, Guid newSessionId);
        ClientData GetSecondClientFromSession(Guid sessionId, ClientData referenceClient);
        string GetOtherClientsIdsToString(ClientData referenceClient);
        int GetNumberOfClientsInSession(Guid sessionId);
        void RemoveClient(ClientData client);
        bool IsSessionFull(Guid sessionId);

        bool IsEmpty();
    }
}
