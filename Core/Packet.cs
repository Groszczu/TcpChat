using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Core
{
    public class Packet
    {
        private const int FormatBytes = 4;

        public HeaderProperty<Guid> Id { get; set; }

        public HeaderProperty<Operation> Operation { get; set; }

        public HeaderProperty<Status> Status { get; set; }

        public HeaderProperty<Timestamp> Timestamp { get; set; }

        public HeaderProperty<int> DestinationId { get; set; }

        public HeaderProperty<int> MessageLength { get; set; }

        public HeaderProperty<string> Message { get; set; }

        public const int MaximumPacketSize = 2048;

        public Packet()
        {
        }

        public Packet(Operation operation, Status status, Guid id, Timestamp timestamp = null)
        {
            SetId(id);
            SetOperation(operation);
            SetStatus(status);
            if (timestamp == null)
                timestamp = new Timestamp(DateTime.UtcNow);
            SetTimestamp(timestamp);

            DestinationId = new HeaderProperty<int>();
            MessageLength = new HeaderProperty<int>();
            Message = new HeaderProperty<string>();
        }

        public IEnumerable<IHeaderProperty> GetSetProperties()
        {
            var allProperties = GetType().GetProperties();
            var setProperties = new HashSet<IHeaderProperty>();

            foreach (var propertyInfo in allProperties)
            {
                var propValue = (IHeaderProperty) propertyInfo.GetValue(this, null);
                var isSet = propValue.IsSet;
                if (isSet)
                {
                    setProperties.Add(propValue);
                }
            }

            return setProperties;
        }

        public Packet SetId(Guid id)
        {
            Id = new HeaderProperty<Guid>(id, "id", 36 + "id".Length + FormatBytes, true);
            return this;
        }

        public Packet SetOperation(Operation operation)
        {
            Operation = new HeaderProperty<Operation>(operation, "operation", 
                1 + "operation".Length + FormatBytes, true);
            return this;
        }

        public Packet SetStatus(Status status)
        {
            Status = new HeaderProperty<Status>(status, "status",
                1 + "status".Length + FormatBytes, true);
            return this;
        }

        public Packet SetTimestamp(Timestamp timestamp)
        {
            Timestamp = new HeaderProperty<Timestamp>(timestamp, "timestamp", 
                16 + "timestamp".Length + FormatBytes, true);
            return this;
        }
        
        public Packet SetDestinationId(int destinationId)
        {
            DestinationId = new HeaderProperty<int>(destinationId, "destination",
                1 + "destination".Length + FormatBytes, true);
            return this;
        }

        public Packet SetMessage(string message)
        {
            message = message.Replace("|", "");
            Message = new HeaderProperty<string>(message, "message",
                65536 + "message".Length + FormatBytes, true);
            var str = (string) Message.ObjectValue;
            MessageLength = new HeaderProperty<int>(str.Length, "length",
                5 + "length".Length + FormatBytes, true);
            return this;
        }
    }
}