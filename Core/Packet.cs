using System;

namespace Core
{
    [Serializable]
    public class Packet
    {
        public Operation Operation { get; set; }
        public Status Status { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }

        public Packet(Operation operation, Status status, int id, string message)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
        }

        public Packet()
        {
            
        }
    }
}
