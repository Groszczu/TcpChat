using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Packet
    {
        public const int MaximumPacketSize = 2048;

        public HeaderProperty<Guid> Id { get; private set; }
        public HeaderProperty<Operation> Operation { get; private set; }
        public HeaderProperty<Status> Status { get; private set; }
        public HeaderProperty<Timestamp> Timestamp { get; set; }
        public HeaderProperty<int> SourceId { get; private set; }
        public HeaderProperty<int> DestinationId { get; private set; }
        public HeaderProperty<string> Message { get; private set; }

        public HeaderProperty<int> Ident { get; private set; }
        public Packet()
        {
            Id = new HeaderProperty<Guid>();
            Status = new HeaderProperty<Status>();
            Timestamp = new HeaderProperty<Timestamp>();
            SourceId = new HeaderProperty<int>();
            DestinationId = new HeaderProperty<int>();
            Message = new HeaderProperty<string>();
        }

        public Packet(Operation operation, Status status, Guid id, int ident, Timestamp timestamp = null)
        {
            SetId(id);
            SetOperation(operation);
            SetStatus(status);
            if (timestamp == null)
                timestamp = new Timestamp(DateTime.UtcNow);
            SetTimestamp(timestamp);
            SetIdent(ident);
            

            DestinationId = new HeaderProperty<int>();
            SourceId = new HeaderProperty<int>();
            Message = new HeaderProperty<string>();
        }

        public IEnumerable<IHeaderProperty> GetSetProperties()
        {
            return GetType().GetProperties()
                .Where(p => ((IHeaderProperty) p.GetValue(this, null)).IsSet)
                .Select(pi => pi.GetValue(this, null) as IHeaderProperty);
//            var setProperties = new HashSet<IHeaderProperty>();
//
//            foreach (var propertyInfo in allProperties)
//            {
//                var propValue = (IHeaderProperty) propertyInfo.GetValue(this, null);
//                var isSet = propValue.IsSet;
//                if (isSet)
//                {
//                    setProperties.Add(propValue);
//                }
//            }
//
//            return setProperties;
        }

        public Packet SetId(Guid id)
        {
            Id = new HeaderProperty<Guid>(id, "sid", true);
            return this;
        }

        public Packet SetOperation(Operation operation)
        {
            Operation = new HeaderProperty<Operation>(operation, "Operacja", true);
            return this;
        }

        public Packet SetStatus(Status status)
        {
            Status = new HeaderProperty<Status>(status, "Status", true);
            return this;
        }

        public Packet SetTimestamp(Timestamp timestamp)
        {
            Timestamp = new HeaderProperty<Timestamp>(timestamp, "timestamp", true);
            return this;
        }

        public Packet SetSourceId(int sourceId)
        {
            SourceId = new HeaderProperty<int>(sourceId, "source", true);
            return this;
        }
        
        public Packet SetDestinationId(int destinationId)
        {
            DestinationId = new HeaderProperty<int>(destinationId, "destination", true);
            return this;
        }

        public Packet SetMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return this;
            RemoveForbiddenSigns(ref message);
            Message = new HeaderProperty<string>(message, "message", true);
            return this;
        }

        private static void RemoveForbiddenSigns(ref string message)
        {
            message = message.Replace("|", "");
        }

        public Packet SetIdent(int ident)
        {
            
            DestinationId = new HeaderProperty<int>(ident, "Identyfikator", true);
            return this;
        }
    }
}