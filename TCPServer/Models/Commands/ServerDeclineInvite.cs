using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerDeclineInvite : ServerCommand
    {
        private readonly int _destinationId;

        public ServerDeclineInvite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
            : base(source, sessionsRepository, packetFormatter, Operation.Invite, Status.Decline)
        {
            _destinationId = destinationId;
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
                throw new InvalidOperationException("Client cannot decline invitation (or invite) himself");
            if (!Source.GotInviteFrom(Destination.Id))
                throw new InvalidOperationException(
                    $"Client have not received an invitation from a client with ID {Destination.Id}");

            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            var sourceSessionId = SessionsRepository.GetSessionId(Source);
            if (DestinationSessionId == sourceSessionId)
                throw new InvalidOperationException($"Client {Destination.Id} is already in your session");
        }

        public override void Execute()
        {
            base.Execute();
            Source.RemoveInvite(Destination.Id);
        }
    }
}