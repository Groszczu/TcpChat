using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDeclineInvite : ClientCommand
    {
        public ClientDeclineInvite(int inviterId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter, int ident) :
            base(sessionId, sender, packetFormatter, Operation.Invite, Status.Decline, ident)
        {
            Packet.SetDestinationId(inviterId);
        }
    }
}