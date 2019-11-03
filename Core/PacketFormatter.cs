using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class PacketFormatter : IPacketFormatter
    {
        private readonly IOrderedEnumerable<PropertyInfo> _orderedProperties;

        public PacketFormatter()
        {
            _orderedProperties = GetOrderedHeaderProperties();
        }

        public byte[] Serialize(Packet packet)
        {
            var stringBuilder = new StringBuilder(MakeHeaderPeace("id", packet.Id.ToString()));
            stringBuilder.Append(MakeHeaderPeace("operation", packet.Operation.ToString("g")));
            stringBuilder.Append(MakeHeaderPeace("status", packet.Status.ToString("g")));
            stringBuilder.Append(MakeHeaderPeace("destination", packet.DestinationId.ToString()));
            stringBuilder.Append(MakeHeaderPeace("length", packet.MessageLength.ToString()));
            stringBuilder.Append(MakeHeaderPeace("message", packet.Message));

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        public async Task<Packet> DeserializeAsyncOld(Stream stream)
        {
            var packet = new Packet();

            foreach (var propertyInfo in _orderedProperties)
            {
                var length = GetPropertyLength(propertyInfo);
                var formatter = GetPropertyFormatter(propertyInfo);

                var buff = new byte[length];
                var remaining = length;

                while (remaining > 0)
                {
                    var dataRead = await stream.ReadAsync(buff, 0, length);
                    if (dataRead <= 0)
                    {
                        throw new EndOfStreamException
                            ($"End of stream reached with {remaining} bytes left to read");
                    }

                    remaining -= dataRead;
                }

                var deserializedPropertyValue = formatter.Read(buff, propertyInfo.PropertyType);

                propertyInfo.SetValue(packet,
                    Convert.ChangeType(deserializedPropertyValue, propertyInfo.PropertyType)
                );
            }

            return packet;
        }

        public async Task<Packet> DeserializeAsync(Stream stream)
        {
            var packet = new Packet();


            var buffer = new byte[Packet.MaximumPacketSize];
            var dataRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var formattedBytes = new byte[dataRead];

            Array.Copy(buffer, formattedBytes, dataRead);

            var serializedMessage = Encoding.UTF8.GetString(formattedBytes);

            var regex = new Regex(@"(?<key>\w+)-\)(?<value>[-\w\s]+)\(\|");
            if (!regex.IsMatch(serializedMessage))
                throw new InvalidDataException("Received message doesn't match the pattern of header");
            foreach (Match match in regex.Matches(serializedMessage))
            {
                switch (match.Groups["key"].Value)
                {
                    case "id":
                        packet.Id = Guid.Parse(match.Groups["value"].Value);
                        break;
                    case "operation":
                        Enum.TryParse(match.Groups["value"].Value, out Operation operation);
                        packet.Operation = operation;
                        break;
                    case "status":
                        Enum.TryParse(match.Groups["value"].Value, out Status status);
                        packet.Status = status;
                        break;
                    case "destination":
                        packet.DestinationId = int.Parse(match.Groups["value"].Value);
                        break;
                    case "length":
                        packet.MessageLength = int.Parse(match.Groups["value"].Value);
                        break;
                    case "message":
                        break;
                    default:
                        throw new InvalidDataException("Received message doesn't match the pattern of header");
                }
            }

            var messageRegex = new Regex(@"^(\w+-\)[-\w\s]+\(\|){5}message-\)(?<message>.+)\(\|$");

            if (!messageRegex.IsMatch(serializedMessage))
                throw new InvalidDataException("Received message doesn't match the pattern of header");

            packet.Message = messageRegex.Match(serializedMessage).Groups["message"].Value;

            return packet;
        }

        private string MakeHeaderPeace(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }

        private int GetPropertyLength(PropertyInfo propertyInfo)
        {
            var headerProperty = (HeaderProperty) propertyInfo.GetCustomAttribute(typeof(HeaderProperty));
            return headerProperty.MaximumLengthInBytes;
        }

        private static IOrderedEnumerable<PropertyInfo> GetOrderedHeaderProperties()
        {
            var properties = typeof(Packet).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(HeaderProperty)));

            var orderedProperties = properties.OrderBy(p =>
            {
                var attribute = (HeaderProperty) p.GetCustomAttribute(typeof(HeaderProperty));
                return attribute.OrderNumber;
            });

            return orderedProperties;
        }

        private static IPropertyFormatter GetPropertyFormatter(MemberInfo propertyInfo)
        {
            var attribute = (HeaderProperty) propertyInfo.GetCustomAttribute(typeof(HeaderProperty));
            return (IPropertyFormatter) Activator.CreateInstance(attribute.PropertyFormatterType, attribute.Key);
        }
    }
}