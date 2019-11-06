using System;
using System.Net.Sockets;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientInvite : ICommand
    {
        public Packet Packet { get; set; }

        private readonly ISender _sender;
        private readonly IPacketFormatter _packetFormatter;

        public ClientInvite(int destinationId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
        {
            _sender = sender;
            _packetFormatter = packetFormatter;

            Packet = new Packet(Operation.Invite, Status.Ok, sessionId);
            Packet.SetDestinationId(destinationId);
        }

        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _sender.Send(serializedMessage);
        }
    }
}