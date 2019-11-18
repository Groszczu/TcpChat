using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDeclineInvite : ClientCommand
    {
        public ClientDeclineInvite(int ident, int inviterId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter) :
            base(ident, sessionId, sender, packetFormatter, Operation.Invite, Status.Decline)
        {
            Packet.SetDestinationId(inviterId);
        }
    }
}