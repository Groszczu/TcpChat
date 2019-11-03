using System;
using System.Linq;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class Invite : ICommand
    {
        public Packet Packet { get; set; }


        private readonly ClientData _source;
        private readonly int _destinationId;
        private readonly ClientData _destination;

        private readonly IPacketFormatter _packetFormatter;
        private readonly Guid _destinationSessionId;
        private readonly Guid _sourceSessionId;

        public Invite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
        {
            _source = source;
            _destinationId = destinationId;
            _packetFormatter = packetFormatter;
            
            _destination = sessionsRepository.GetClientById(destinationId);
            _destinationSessionId = sessionsRepository.GetSessionId(_destination);
            _sourceSessionId = sessionsRepository.GetSessionId(_source);
            ValidateInvite();

            Packet = new Packet(Operation.Invite, Status.Ok, _destinationSessionId, GenerateMessage(), _destinationId);
        }

        public void Execute()
        {
            var stream = _destination.Socket.GetStream();
            var serializedMessage = _packetFormatter.Serialize(Packet);
            stream.Write(serializedMessage, 0, serializedMessage.Length);
        }

        private void ValidateInvite()
        {
            if (_destinationId == _source.Id)
                throw new InvalidOperationException("User cannot invite himself to session");

            if (_destination == null)
                throw new InvalidOperationException($"There is no user with ID: {_destinationId}");
        }

        private string GenerateMessage()
        {
            var baseMessage = $"You (client {_destinationId}) got invite from client {_source.Id} to session {_sourceSessionId}";
            return baseMessage;
        }
    }
}