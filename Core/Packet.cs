using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Core
{
    public class Packet 
    {
        [HeaderProperty(1, 16, typeof(HeaderPropertyFormatter))]
        public Guid Id { get; private set; }
        [HeaderProperty(2, 4, typeof(HeaderPropertyFormatter))]
        public Operation Operation { get; private set; }
        [HeaderProperty(3, 4, typeof(HeaderPropertyFormatter))]
        public Status Status { get; private set; }
        [HeaderProperty(4, 4, typeof(HeaderPropertyFormatter))]
        public int MessageLength { get; private set; }
        public string Message { get; private set; }

        public Packet() { }
        public Packet(Operation operation, Status status, Guid id, string message)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
        }
    }
}
