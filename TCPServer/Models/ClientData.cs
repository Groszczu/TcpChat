using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Sockets;
using TCPServer.Services;

namespace TCPServer.Models
{
    public class ClientData
    {
        public int Id { get; }
        public TcpClient Socket { get; }

        private readonly Dictionary<int, Guid> _pendingInvites = new Dictionary<int, Guid>();

        public ClientData(int id, TcpClient socket)
        {
            Id = id;
            Socket = socket;
        }

        public void AddNewInvite(int inviterId, Guid inviterSessionId)
        {
            _pendingInvites[inviterId] = inviterSessionId;
        }

        public void RemoveInvite(int inviterId)
        {
            if (!_pendingInvites.ContainsKey(inviterId))
                throw new InvalidOperationException($"No pending invite for client {Id} from client {inviterId}");
            _pendingInvites.Remove(inviterId);
        }

        public void SendTo(byte[] bytes)
        {
            var stream = Socket.GetStream();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
