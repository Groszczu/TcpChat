using System;
using System.Net.Sockets;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientInvite : ICommand
    {
        public Packet Packet { get; set; }

        private readonly NetworkStream _stream;
        private readonly IPacketFormatter _packetFormatter;

        public ClientInvite(int destinationId, Guid sessionId, NetworkStream stream, IPacketFormatter packetFormatter)
        {
            _stream = stream;
            _packetFormatter = packetFormatter;
            
            Packet = new Packet(Operation.Invite, Status.Ok, sessionId, "No message", destinationId);
        }
        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _stream.Write(serializedMessage, 0, serializedMessage.Length);
        }
    }
}