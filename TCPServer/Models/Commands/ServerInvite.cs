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
            : base(source, sessionsRepository, packetFormatter, Operation.Invite, Status.Ok)
        {
            _destinationId = destinationId;
            _sourceSessionId = SessionsRepository.GetSessionId(Source);
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
                throw new InvalidOperationException("You cannot invite yourself to the session");

            if (SessionsRepository.IsSessionFull(_sourceSessionId))
                throw new InvalidOperationException("Your session is full");
            
            if (SessionsRepository.IsSessionFull(DestinationSessionId))
                throw new InvalidOperationException($"Client's {Destination.Id} session is full");

            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            if (DestinationSessionId == _sourceSessionId)
                throw new InvalidOperationException("User you are inviting is already in your session");
        }

        protected override void GenerateAndSetMassage()
        {
            var message = $"You (client {Destination.Id}) got invite from client {Source.Id} to session {_sourceSessionId}";
            Packet.SetMessage(message);
        }

        public override void Execute()
        {
            base.Execute();
            Destination.AddNewInvite(Source.Id, _sourceSessionId);
        }
    }
}