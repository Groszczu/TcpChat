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
        private ClientData _destination;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IPacketFormatter _packetFormatter;
        private readonly Guid _destinationSessionId;
        private readonly Guid _sourceSessionId;

        public ServerInvite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
        {
            _source = source;
            _sessionsRepository = sessionsRepository;
            _packetFormatter = packetFormatter;

            ValidateClientIdAndInitialize(destinationId);
            _destinationSessionId = sessionsRepository.GetSessionId(_destination);
            _sourceSessionId = sessionsRepository.GetSessionId(_source);
            ValidateInvite();

            Packet = new Packet(Operation.Invite, Status.Ok, _destinationSessionId)
                .SetDestinationId(_destination.Id)
                .SetMessage(GenerateMessage());
        }

        private void ValidateClientIdAndInitialize(int destinationId)
        {
            try
            {
                _destination = _sessionsRepository.GetClientById(destinationId);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"There is no user with ID: {destinationId}");
            }
        }

        public void Execute()
        {
            _destination.AddNewInvite(_source.Id, _sourceSessionId);
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _destination.SendTo(serializedMessage);
        }

        private void ValidateInvite()
        {
            if (_destination.Id == _source.Id)
                throw new InvalidOperationException("User cannot invite himself to session");

            if (_destinationSessionId == _sourceSessionId)
                throw new InvalidOperationException("User you are inviting is already in your session");
        }

        private string GenerateMessage()
        {
            var baseMessage =
                $"You (client {_destination.Id}) got invite from client {_source.Id} to session {_sourceSessionId}";
            return baseMessage;
        }
    }
}