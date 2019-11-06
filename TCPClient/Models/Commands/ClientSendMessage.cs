using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientSendMessage : ICommand
    {
        private readonly ISender _sender;
        private readonly IPacketFormatter _packetFormatter;
        public Packet Packet { get; set; }

        public ClientSendMessage(Guid sessionId, ISender sender, IPacketFormatter packetFormatter,
            string messageToSend)
        {
            _sender = sender;
            _packetFormatter = packetFormatter;

            Packet = new Packet(Operation.Message, Status.Ok, sessionId);
            Packet.SetMessage(messageToSend);
        }

        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _sender.Send(serializedMessage);
        }
    }
}