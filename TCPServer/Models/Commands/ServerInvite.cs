using System;
using System.Linq;
using System.Net.Sockets;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerInvite : ICommand
    {
        public Packet Packet { get; set; }

        private readonly ClientData _source;
        private readonly int _destinationId;
        private readonly ClientData _destination;

        private readonly IPacketFormatter _packetFormatter;
        private readonly Guid _destinationSessionId;
        private readonly Guid _sourceSessionId;

        public ServerInvite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
        {
            _source = source;
            _destinationId = destinationId;
            _packetFormatter = packetFormatter;

            _destination = sessionsRepository.GetClientById(destinationId);
            ValidateClientId();
            _destinationSessionId = sessionsRepository.GetSessionId(_destination);
            _sourceSessionId = sessionsRepository.GetSessionId(_source);
            ValidateInvite();

            Packet = new Packet(Operation.Invite, Status.Ok, _destinationSessionId)
                .SetMessage(GenerateMessage())
                .SetDestinationId(_destinationId);
        }

        private void ValidateClientId()
        {
            if (_destination == null)
                  throw new InvalidOperationException($"There is no user with ID: {_destinationId}");
        }

        public void Execute()
        {
            _destination.AddNewInvite(_source.Id, _sourceSessionId);
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _destination.SendTo(serializedMessage);
        }

        private void ValidateInvite()
        {
            if (_destinationId == _source.Id)
                throw new InvalidOperationException("User cannot invite himself to session");

            if (_destinationSessionId == _sourceSessionId)
                throw new InvalidOperationException("Invited user is in your session");

            
        }

        private string GenerateMessage()
        {
            var baseMessage =
                $"You (client {_destinationId}) got invite from client {_source.Id} to session {_sourceSessionId}";
            return baseMessage;
        }
    }
}