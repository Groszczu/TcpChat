using System;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerSendMessage : ServerCommand
    {
        private readonly string _messageToSend;
        private readonly Guid _sourceSessionId;

        public ServerSendMessage(ClientData source, ISessionsRepository sessionsRepository,
            IPacketFormatter packetFormatter, string messageToSend)
        : base(source, sessionsRepository, packetFormatter, Operation.Message, Status.Ok)
        {
            _messageToSend = messageToSend;
            _sourceSessionId = SessionsRepository.GetSessionId(Source);
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            
            ValidateAndInitializeDestination();
        }

        private void ValidateAndInitializeDestination()
        {
            if (SessionsRepository.GetNumberOfClientsInSession(_sourceSessionId) == 1)
                throw new InvalidOperationException("There is no other client in your session");
            if (SessionsRepository.GetNumberOfClientsInSession(_sourceSessionId) != 2)
                throw new InvalidOperationException("Error, try again");
            
            try
            {
                Destination = SessionsRepository.GetSecondClientFromSession(_sourceSessionId, Source);
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Error, try again");
            }

            DestinationSessionId = _sourceSessionId;
        }

        protected override void GenerateAndSetMassage()
        {
            var message = $"Client {Source.Id}: {_messageToSend}";
            Packet.SetMessage(message);
        }
    }
}