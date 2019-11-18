using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientInvite : ClientCommand
    {
        public ClientInvite(int destinationId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter, int ident)
            : base(sessionId, sender, packetFormatter, Operation.Invite, Status.Ok, ident)
        {
            Packet.SetDestinationId(destinationId);
        }
    }
}