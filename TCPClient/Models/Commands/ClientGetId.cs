using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientGetId : ClientCommand
    {
        public ClientGetId(int ident, Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
            : base(ident, sessionId, sender, packetFormatter, Operation.GetId, Status.Ok)
        {
        }
    }
}