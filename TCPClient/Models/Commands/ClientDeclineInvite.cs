﻿using System;
using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDeclineInvite : ICommand
    {
        private readonly ISender _sender;
        private readonly IPacketFormatter _packetFormatter;
        public Packet Packet { get; set; }

        public ClientDeclineInvite(int inviterId, Guid sessionId, ISender sender, IPacketFormatter packetFormatter)
        {
            _sender = sender;
            _packetFormatter = packetFormatter;
            Packet = new Packet(Operation.Invite, Status.Decline, sessionId).SetDestinationId(inviterId);
        }

        public void Execute()
        {
            var serializedMessage = _packetFormatter.Serialize(Packet);
            _sender.Send(serializedMessage);
        }
    }
}