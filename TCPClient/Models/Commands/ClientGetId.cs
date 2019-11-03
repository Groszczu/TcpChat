using System;
using System.IO;
using System.Net.Sockets;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientGetId : ICommand
    {
        public Packet Packet { get; set; }
        private readonly NetworkStream _stream;
        private readonly IPacketFormatter _packetFormatter;

        public ClientGetId(Guid sessionId, NetworkStream stream, IPacketFormatter packetFormatter)
        {
            _stream = stream;
            _packetFormatter = packetFormatter;
            Packet = new Packet(Operation.GetId, Status.Ok, sessionId, "No message");
        }

        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _stream.Write(serializedMessage, 0, serializedMessage.Length);
        }
    }
}