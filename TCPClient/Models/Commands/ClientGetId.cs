using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientGetId : ClientCommand
    {
        public ClientGetId(Guid sessionId, ISender sender, IPacketFormatter packetFormatter, int ident)
            : base(sessionId, sender, packetFormatter, Operation.GetId, Status.Ok, ident)
        {
        }
    }
}