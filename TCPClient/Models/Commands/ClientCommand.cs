using System;
using Core;

namespace TCPClient.Models.Commands
{
    public abstract class ClientCommand : ICommand
    {
        public Packet Packet { get; set; }

        private readonly ISender _sender;
        private readonly IPacketFormatter _packetFormatter;

        protected ClientCommand(Guid sessionId, ISender sender, IPacketFormatter packetFormatter,
            Operation operation, Status status, int identyfikator)
        {
            _sender = sender;
            _packetFormatter = packetFormatter;
            Packet = new Packet(operation, status, sessionId, identyfikator);
        }

        public virtual void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _sender.Send(serializedMessage);
        }
    }
}