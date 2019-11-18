using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientInvite : ClientCommand
    {
        public ClientInvite(int ident, int destinationId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
            : base(ident, sessionId, sender, packetFormatter, Operation.Invite, Status.Ok)
        {
            Packet.SetDestinationId(destinationId);
        }
    }
}