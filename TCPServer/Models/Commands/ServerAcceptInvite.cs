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
                throw new InvalidOperationException($"There is no client with ID: {_destinationId}");
            }
        }

        private void ValidateAndInitializeSessionIds()
        {
            if (Destination == Source)
                throw new InvalidOperationException("Client cannot accept invitation (or invite) himself");
            if (!Source.GotInviteFrom(Destination.Id))
                throw new InvalidOperationException(
                    $"Client have not received an invitation from a client with ID {Destination.Id}");

            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            var sourceSessionId = SessionsRepository.GetSessionId(Source);
            if (DestinationSessionId == sourceSessionId)
                throw new InvalidOperationException($"Client {Destination.Id} is already in your session");
            
            if (SessionsRepository.IsSessionFull(sourceSessionId))
                throw new InvalidOperationException("Client's session is full");
            if (SessionsRepository.IsSessionFull(DestinationSessionId))
                throw new InvalidOperationException("Inviter session is full");
            
            SessionsRepository.UpdateClientSessionId(Source, DestinationSessionId);
        }

        protected override void SetPacketFields()
        {
            Packet.SetSourceId(Source.Id);
        }

        public override void Execute()
        {
            base.Execute();
            Source.RemoveAllPendingInvites();
            Destination.RemoveAllPendingInvites();

            var sourceNewSessionId = SessionsRepository.GetSessionId(Source);
            var initialPacket = new Packet(Operation.GetId, Status.Initial, sourceNewSessionId, Source.Id);
            Source.SendTo(PacketFormatter.Serialize(initialPacket));
        }
    }
}