using System;
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
        
        [HeaderProperty(1, "id", 36 + 2 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Guid Id { get; set; }

        [HeaderProperty(2, "operation",  12 + 9 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Operation Operation { get; set; }

        [HeaderProperty(3, "status",   12 + 6 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public Status Status { get; set; }

        [HeaderProperty( 4, "destination",    1 + 11 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public int DestinationId { get; set; }

        [HeaderProperty(5, "length",  5 + 6 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public int MessageLength { get; set; }

        [HeaderProperty( 6, "message",  65536 + 7 + FormatBytes, typeof(HeaderPropertyFormatter))]
        public string Message { get; set; }

        public static int MaximumPacketSize => GetMaximumPacketSize();

        public Packet()
        {
        }

        public Packet(Operation operation, Status status, Guid id, string message, int destinationId = -1)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
            MessageLength = message.Length;
            DestinationId = destinationId;
        }

        private static int GetMaximumPacketSize()
        {
            var headerProperties = typeof(Packet).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(HeaderProperty)));
            
            
            var maximumLengthOfHeaderProperties =  headerProperties
                .Select(propertyInfo => (HeaderProperty) propertyInfo.GetCustomAttribute(typeof(HeaderProperty)))
                .Select(attribute => attribute.MaximumLengthInBytes)
                .Sum();
            return maximumLengthOfHeaderProperties;
        }
    }
}