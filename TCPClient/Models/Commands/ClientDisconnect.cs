﻿using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDisconnect : ClientCommand
    {
        public ClientDisconnect(Guid sessionId, ISender sender, IPacketFormatter packetFormatter) 
            : base(sessionId, sender, packetFormatter, Operation.Disconnect, Status.Ok)
        {
        }
    }
}