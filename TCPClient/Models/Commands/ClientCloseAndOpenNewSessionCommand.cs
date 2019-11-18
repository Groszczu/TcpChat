using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientCloseAndOpenNewSessionCommand : ClientCommand
    {
        public ClientCloseAndOpenNewSessionCommand(int ident, Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
            : base(ident, sessionId, sender, packetFormatter, Operation.CloseSession, Status.Ok)
        {
        }
    }
}