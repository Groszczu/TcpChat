using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Core
{
    public class Packet 
    {
        public Operation Operation { get; set; }
        public Status Status { get; set; }
        public Guid Id { get; set; }
        public string Message { get; set; }

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
