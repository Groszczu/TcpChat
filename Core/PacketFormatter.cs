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

                var deserializedPropertyValue = formatter.Read(buff, propertyInfo.PropertyType);
                
                propertyInfo.SetValue(packet,
                    Convert.ChangeType(deserializedPropertyValue, propertyInfo.PropertyType)
                );
            }

            return packet;
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
            return (IPropertyFormatter) Activator.CreateInstance(attribute.PropertyFormatterType, attribute.Key);
        }
    }
}