using System;
using System.IO;
using System.Net.Sockets;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientGetId : ClientCommand
    {
        public ClientGetId(Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
            : base(sessionId, sender, packetFormatter, Operation.GetId, Status.Ok)
        {
        }
    }
}