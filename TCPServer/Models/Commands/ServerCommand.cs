using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public abstract class ServerCommand : ICommand
    {
        public Packet Packet { get; set; }
        
        protected readonly ClientData Source;
        protected ClientData Destination;
        protected Guid DestinationSessionId;
        protected readonly ISessionsRepository SessionsRepository;
        
        private readonly IPacketFormatter _packetFormatter;
        private readonly Operation _operation;
        private readonly Status _status;

        protected ServerCommand(ClientData source, ISessionsRepository sessionsRepository, IPacketFormatter packetFormatter,
            Operation operation, Status status)
        {
            Source = source;
            SessionsRepository = sessionsRepository;
            _packetFormatter = packetFormatter;
            _operation = operation;
            _status = status;
        }

        protected abstract void ValidateAndInitializeCommandArguments();

        protected abstract void GenerateAndSetMassage();

        public virtual void Execute()
        {
            ValidateAndInitializeCommandArguments();
            Packet = new Packet(_operation, _status, DestinationSessionId);
            GenerateAndSetMassage();
            
            var serializedPacket = _packetFormatter.Serialize(Packet);
            Destination.SendTo(serializedPacket);
        }
    }
}