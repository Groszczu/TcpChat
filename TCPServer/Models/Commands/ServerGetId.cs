using System;
using System.Linq;
using System.Text;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerGetId : ICommand
    {
        public Packet Packet { get; set; }

        private readonly ClientData _client;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IPacketFormatter _packetFormatter;
        private readonly Guid _sessionId;

        public ServerGetId(ClientData client, ISessionsRepository sessionsRepository, IPacketFormatter packetFormatter)
        {
            _client = client;
            _sessionsRepository = sessionsRepository;
            _packetFormatter = packetFormatter;
            _sessionId = sessionsRepository.GetSessionId(client);

            Packet = new Packet(Operation.GetId, Status.Ok, _sessionId, GenerateMessage());
        }

        public void Execute()
        {
            var stream = _client.Socket.GetStream();
            var serializedMessage = _packetFormatter.Serialize(Packet);
            stream.Write(serializedMessage, 0, serializedMessage.Length);
        }

        private string GenerateMessage()
        {
            var message = new StringBuilder($"Your client ID: {_client.Id}, Your session ID: \'{_sessionId}\'");
            var otherClients = _sessionsRepository.GetAllClients().Where(client => client.Id != _client.Id).ToArray();

            if (otherClients.Length == 0)
            {
                message.Append(" No other clients connected to server");
            }
            else
            {
                message.Append(" Other client IDs: ");

                var delimiter = string.Empty;
                foreach (var clientData in otherClients)
                {
                    message.Append(delimiter + clientData.Id);
                    delimiter = ", ";
                }
            }
            return message.ToString();
        }
    }
}