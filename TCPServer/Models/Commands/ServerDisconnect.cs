using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerDisconnect : ServerCommand
    {
        public ServerDisconnect(ClientData source, ISessionsRepository sessionsRepository, IPacketFormatter packetFormatter) 
            : base(source, sessionsRepository, packetFormatter, Operation.Disconnect, Status.Ok)
        {
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            Destination = Source;
            DestinationSessionId = SessionsRepository.GetSessionId(Destination);
            SessionsRepository.RemoveClient(Destination);
        }
    }
}