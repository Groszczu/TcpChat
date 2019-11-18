using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDisconnect : ClientCommand
    {
        public ClientDisconnect(int ident, Guid sessionId, ISender sender, IPacketFormatter packetFormatter) 
            : base(ident, sessionId, sender, packetFormatter, Operation.Disconnect, Status.Ok)
        {
        }
    }
}