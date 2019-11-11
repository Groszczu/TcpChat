using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerCloseAndOpenNewSessionCommand : ServerCommand
    {
        public ServerCloseAndOpenNewSessionCommand(ClientData source, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter) 
            : base(source, sessionsRepository, packetFormatter, Operation.CloseSession, Status.Ok)
        {
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            Destination = Source;
            DestinationSessionId = SessionsRepository.GetSessionId(Source);

            // source is only client in session
            if (!SessionsRepository.IsSessionFull(DestinationSessionId))
                throw new InvalidOperationException(
                    "Closing communication session and moving to new one is possible only if other client is in your session");
            
            SessionsRepository.UpdateClientSessionId(Destination, Guid.NewGuid());
        }

        protected override void GenerateAndSetMassage()
        {
            var newSessionId = SessionsRepository.GetSessionId(Destination);

            var message = $"You were moved to the new session '{newSessionId}'";
            Packet.SetMessage(message);
        }
    }
}