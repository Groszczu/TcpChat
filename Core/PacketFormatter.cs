using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
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
            var str = MakeHeaderPeace("operation", packet.Operation.ToString("g"))
                      + MakeHeaderPeace("status", packet.Status.ToString("g"))
                      + MakeHeaderPeace("id", packet.Id.ToString())
                      + " " + packet.Message;

            return Encoding.UTF8.GetBytes(str);
        }

        public async Task<Packet> DeserializeAsync(Stream stream)
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

                var deserializedPropertyValue = formatter.Read(buff);

                propertyInfo.SetValue(packet, deserializedPropertyValue);
            }

            return packet;

//            var message = Encoding.UTF8.GetString(array);
//            var regex = new Regex(@"(?<key>\w+)-\)(?<value>\w+)\(\|");
//            if (!regex.IsMatch(message))
//                throw new InvalidDataException("Received message doesn't match the pattern of header");
//
//            foreach (Match match in regex.Matches(message))
//            {
//                switch (match.Groups["key"].Value)
//                {
//                    case "operation":
//                        Enum.TryParse(match.Groups["value"].Value, out Operation opr);
//                        data.Operation = opr;
//                        break;
//                    case "status":
//                        Enum.TryParse(match.Groups["value"].Value, out Status status);
//                        data.Status = status;
//                        break;
//                    case "id":
//                        data.Id = Guid.Parse(match.Groups["value"].Value);
//                        break;
//                    default:
//                        throw new InvalidDataException("Received message doesn't match the pattern of header");
//                }
//            }
//
//            regex = new Regex(@"(\w+-\)(\w|-)+\(\|){3}\s*(?<message>.*)");
//            data.Message = !regex.IsMatch(message) ? "Empty message" : regex.Match(message).Groups["message"].Value;
//
//            return data;
        }

        private string MakeHeaderPeace(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }

        private int GetPropertyLength(PropertyInfo propertyInfo)
        {
            var headerProperty = (HeaderProperty) propertyInfo.GetCustomAttribute(typeof(HeaderProperty));
            return headerProperty.LengthInBytes;
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
            return (IPropertyFormatter) Activator.CreateInstance(attribute.PropertyFormatterType);
        }
    }
}