using System;
using System.Collections.Generic;

namespace Core
{
    public class Packet
    {
        public const int MaximumPacketSize = 2048;

        public HeaderProperty<Guid> Id { get; private set; }
        public HeaderProperty<Operation> Operation { get; private set; }
        public HeaderProperty<Status> Status { get; private set; }
        public HeaderProperty<Timestamp> Timestamp { get; set; }
        public HeaderProperty<int> DestinationId { get; private set; }
        public HeaderProperty<int> MessageLength { get; private set; }
        public HeaderProperty<string> Message { get; private set; }

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
            Id = new HeaderProperty<Guid>(id, "id", true);
            return this;
        }

        public Packet SetOperation(Operation operation)
        {
            Operation = new HeaderProperty<Operation>(operation, "operation", true);
            return this;
        }

        public Packet SetStatus(Status status)
        {
            Status = new HeaderProperty<Status>(status, "status", true);
            return this;
        }

        public Packet SetTimestamp(Timestamp timestamp)
        {
            Timestamp = new HeaderProperty<Timestamp>(timestamp, "timestamp", true);
            return this;
        }
        
        public Packet SetDestinationId(int destinationId)
        {
            DestinationId = new HeaderProperty<int>(destinationId, "destination", true);
            return this;
        }

        public Packet SetMessage(string message)
        {
            RemoveForbiddenSigns(ref message);
            Message = new HeaderProperty<string>(message, "message", true);
            var str = (string) Message.ObjectValue;
            MessageLength = new HeaderProperty<int>(str.Length, "length", true);
            return this;
        }

        private void RemoveForbiddenSigns(ref string message)
        {
            message = message.Replace("|", "");
        }
    }
}