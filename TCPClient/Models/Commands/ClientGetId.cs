using System;
using System.IO;
using System.Net.Sockets;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientGetId : ICommand
    {
        public Packet Packet { get; set; }
        private readonly ISender _sender;
        private readonly IPacketFormatter _packetFormatter;

        public ClientGetId(Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
        {
            _sender = sender;
            _packetFormatter = packetFormatter;
            Packet = new Packet(Operation.GetId, Status.Ok, sessionId);
        }

        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _sender.Send(serializedMessage);
        }
    }
}