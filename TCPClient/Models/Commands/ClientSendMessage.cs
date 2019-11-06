using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientSendMessage : ClientCommand
    {
        public ClientSendMessage(Guid sessionId, ISender sender, IPacketFormatter packetFormatter,
            string messageToSend)
            : base(sessionId, sender, packetFormatter, Operation.Message, Status.Ok)
        {
            Packet.SetMessage(messageToSend);
        }
    }
}