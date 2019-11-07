﻿using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientCloseAndOpenNewSessionCommand : ClientCommand
    {
        public ClientCloseAndOpenNewSessionCommand(Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
            : base(sessionId, sender, packetFormatter, Operation.CloseSession, Status.Ok)
        {
        }
    }
}