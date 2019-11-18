using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientAcceptInvite : ClientCommand
    {
        public ClientAcceptInvite(int inviterId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter, int ident) 
            : base(sessionId, sender, packetFormatter, Operation.Invite, Status.Accept, ident)
        {
            Packet.SetDestinationId(inviterId);
        }
    }
}