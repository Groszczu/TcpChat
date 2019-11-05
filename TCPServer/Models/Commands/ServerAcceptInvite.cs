using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerAcceptInvite : ICommand
    {
        private readonly ClientData _source;
        private ClientData _inviter;
        private readonly ISessionsRepository _sessionsRepository;
        private readonly IPacketFormatter _packetFormatter;
        private Guid _inviterSessionId;
        private Guid _sourceSessionId;
        public Packet Packet { get; set; }

        public ServerAcceptInvite(ClientData source, int inviterId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
        {
            _source = source;
            _sessionsRepository = sessionsRepository;
            _packetFormatter = packetFormatter;
            
            ValidateInviterIdAndInitialize(inviterId);
            ValidateAndInitializeSessionIds();
            
            Packet = new Packet(Operation.Invite, Status.Accept, _inviterSessionId)
                .SetDestinationId(_inviter.Id)
                .SetMessage(GenerateMessage());
        }

        private string GenerateMessage()
        {
            var message =
                $"Client with ID {_source.Id} accepted your invite. You can chat now using '-m [message]' tag";
            return message;
        }

        private void ValidateAndInitializeSessionIds()
        {
            if (_inviter.Id == _source.Id)
                throw new InvalidOperationException("You cannot accept invitation (or invite) yourself");
            if (!_source.GotInviteFrom(_inviter.Id))
                throw new InvalidOperationException($"You have not received an invitation from a client with ID {_inviter.Id}");
            
            _sourceSessionId = _sessionsRepository.GetSessionId(_source);
            _inviterSessionId = _sessionsRepository.GetSessionId(_inviter);
        }

        private void ValidateInviterIdAndInitialize(int inviterId)
        {
            try
            {
                _inviter = _sessionsRepository.GetClientById(inviterId);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"There is no user with ID: {inviterId}");
            }
        }

        public void Execute()
        {
            _sessionsRepository.UpdateClientSessionId(_source, _inviterSessionId);
            _source.RemoveInvite(_inviter.Id);
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _inviter.SendTo(serializedMessage);
        }
    }
}