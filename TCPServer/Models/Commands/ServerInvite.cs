using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerInvite : ServerCommand
    {
        private readonly int _destinationId;
        private readonly Guid _sourceSessionId;

        public ServerInvite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
            : base(source, sessionsRepository, packetFormatter, Operation.Invite, Status.Ok, destinationId)
        {
            _destinationId = destinationId;
            _sourceSessionId = SessionsRepository.GetSessionId(Source);
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            ValidateAndInitializeDestinationId();
            ValidateAndInitializeSessionIds();
        }

        protected override void SetPacketFields()
        {
            Packet.SetSourceId(Source.Id);
        }

        private void ValidateAndInitializeDestinationId()
        {
            try
            {
                Destination = SessionsRepository.GetClientById(_destinationId);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"There is no user with ID: {_destinationId}");
            }
        }

        private void ValidateAndInitializeSessionIds()
        {
            if (Destination == Source)
                throw new InvalidOperationException("Client cannot invite himself to the session");

            if (SessionsRepository.IsSessionFull(_sourceSessionId))
                throw new InvalidOperationException("Session is full");
            
            if (SessionsRepository.IsSessionFull(DestinationSessionId))
                throw new InvalidOperationException($"Client's {Destination.Id} session is full");
            
            if (Destination.GotInviteFrom(Source.Id))
                throw new InvalidOperationException(
                    $"Already invited client with ID: {Destination.Id}");

            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            if (DestinationSessionId == _sourceSessionId)
                throw new InvalidOperationException("Invited client is already in this session");
        }

        public override void Execute()
        {
            base.Execute();
            Destination.AddNewInvite(Source.Id, _sourceSessionId);
        }
    }
}