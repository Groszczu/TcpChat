using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Core
{
    [Serializable]
    public class Packet : ISerializable
    {
        public Operation Operation { get; set; }
        public Status Status { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }

        public Packet() { }
        public Packet(Operation operation, Status status, int id, string message)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
        }

        protected Packet(SerializationInfo info, StreamingContext context)
        {
            Operation = (Operation) info.GetInt32("operation");
            Status = (Status) info.GetInt32("status");
            Id = info.GetInt32("id");
            Message = info.GetString("message");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AssemblyName = "";
            info.FullTypeName = "";
            info.AddValue("operation", Operation);
            info.AddValue("status", Status);
            info.AddValue("id", Id);
            info.AddValue("message", Message);
        }
    }
}
