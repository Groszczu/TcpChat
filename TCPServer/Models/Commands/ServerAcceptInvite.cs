using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerAcceptInvite : ServerCommand
    {
        private readonly int _destinationId;

        public ServerAcceptInvite(ClientData source, int destinationId, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter)
            : base(source, sessionsRepository, packetFormatter, Operation.Invite, Status.Accept)
        {
            _destinationId = destinationId;
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            ValidateAndInitializeDestinationId();
            ValidateAndInitializeSessionIds();
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
                throw new InvalidOperationException("You cannot accept invitation (or invite) yourself");
            if (!Source.GotInviteFrom(Destination.Id))
                throw new InvalidOperationException(
                    $"You have not received an invitation from a client with ID {Destination.Id}");

            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            var sourceSessionId = SessionsRepository.GetSessionId(Source);
            if (DestinationSessionId == sourceSessionId)
                throw new InvalidOperationException("User you are inviting is already in your session");
            
            if (SessionsRepository.IsSessionFull(sourceSessionId))
                throw new InvalidOperationException("Your session is full");
            if (SessionsRepository.IsSessionFull(DestinationSessionId))
                throw new InvalidOperationException($"Client's {Destination.Id} session is full");
        }

        protected override void GenerateAndSetMassage()
        {
            var message = $"Client with ID {Source.Id} accepted your invite. You can chat now";
            Packet.SetMessage(message);
        }

        public override void Execute()
        {
            base.Execute();
            SessionsRepository.UpdateClientSessionId(Source, DestinationSessionId);
            Source.RemoveAllPendingInvites();
        }
    }
}