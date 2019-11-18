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
            IPacketFormatter packetFormatter, string messageToSend, Guid sourceSessionId)
        : base(source, sessionsRepository, packetFormatter, Operation.Message, Status.Ok, sessionsRepository.GetSecondClientFromSession(sourceSessionId, source).Id)
        {
            _messageToSend = messageToSend;
            _sourceSessionId = SessionsRepository.GetSessionId(Source);
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            ValidateAndInitializeDestination();
        }

        protected override void SetPacketFields()
        {
            Packet.SetSourceId(Source.Id);
            Packet.SetMessage(_messageToSend);
        }

        private void ValidateAndInitializeDestination()
        {
            if (!SessionsRepository.IsSessionFull(_sourceSessionId))
                throw new InvalidOperationException("No other client in client's session");
            
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
    }
}