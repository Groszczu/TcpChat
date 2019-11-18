using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerGetId : ServerCommand
    {
        public ServerGetId(ClientData source, ISessionsRepository sessionsRepository, IPacketFormatter packetFormatter)
            : base(source, sessionsRepository, packetFormatter, Operation.GetId, Status.Ok, source.Id)
        {
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            Destination = Source;
            DestinationSessionId = SessionsRepository.GetSessionId(Source);
        }

        protected override void SetPacketFields()
        {
            var otherIdsMessage = SessionsRepository.GetOtherClientsIdsToString(Source);
            Packet.SetMessage(otherIdsMessage);

            Packet.SetDestinationId(Destination.Id);
        }
    }
}