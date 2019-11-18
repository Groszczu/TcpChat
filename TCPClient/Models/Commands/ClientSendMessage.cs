using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientSendMessage : ClientCommand
    {
        public ClientSendMessage(int ident, Guid sessionId, ISender sender,
            IPacketFormatter packetFormatter, string messageToSend)
            : base(ident, sessionId, sender, packetFormatter, Operation.Message, Status.Ok)
        {
            Packet.SetMessage(messageToSend);
        }
    }
}