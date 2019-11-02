using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Core
{
    public class Packet
    {
        private const int FormatBytes = 4;

        [HeaderProperty(1, "id", 36 + 2 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Guid Id { get; private set; }

        [HeaderProperty(2, "operation",  1 + 9 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Operation Operation { get; private set; }

        [HeaderProperty(3, "status",   1 + 6 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Status Status { get; private set; }

        [HeaderProperty(4, "length",  5 + 6 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public int MessageLength { get; private set; }

        [DynamicProperty( "message", 7 + FormatBytes, "length", typeof(HeaderPropertyFormatter))]
        public string Message { get; private set; }

        public Packet()
        {
        }

        public Packet(Operation operation, Status status, Guid id, string message)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
            MessageLength = message.Length;
        }
    }
}