﻿using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Core;
using TCPServer.Services;

namespace TCPServer.Models.Commands
{
    public class ServerGetId : ServerCommand
    {
        public ServerGetId(ClientData source, ISessionsRepository sessionsRepository, IPacketFormatter packetFormatter)
            : base(source, sessionsRepository, packetFormatter, Operation.GetId, Status.Ok)
        {
            Destination = source;
        }

        protected override void ValidateAndInitializeCommandArguments()
        {
            DestinationSessionId = SessionsRepository.GetSessionId(Source);
        }

        protected override void GenerateAndSetMassage()
        {
            var message =
                new StringBuilder($"Your client ID: {Source.Id}, Your session ID: \'{DestinationSessionId}\'");
            var otherIdsMessage = SessionsRepository.GetOtherClientsIdsToString(Source);
            message.Append(otherIdsMessage);
            Packet.SetMessage(message.ToString());
        }
    }
}