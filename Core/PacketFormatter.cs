using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    public class PacketFormatter : IPacketFormatter
    {
        public byte[] Serialize(Packet packet)
        {
            var stringBuilder = new StringBuilder();
            foreach (var property in packet.GetSetProperties())
            {
                stringBuilder.Append(MakeHeaderPeace(property.Key, property.ObjectValue.ToString()));
            }

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
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
                        packet.SetId(Guid.Parse(match.Groups["value"].Value));
                        break;
                    case "operation":
                        Enum.TryParse(match.Groups["value"].Value, out Operation operation);
                        packet.SetOperation(operation);
                        break;
                    case "status":
                        Enum.TryParse(match.Groups["value"].Value, out Status status);
                        packet.SetStatus(status);
                        break;
                    case "timestamp":
                        var unixTimestamp = int.Parse(match.Groups["value"].Value);
                        packet.SetTimestamp(new Timestamp(unixTimestamp));
                        break;
                    case "destination":
                        packet.SetDestinationId(int.Parse(match.Groups["value"].Value));
                        break;
                    case "length":
                        break;
                    case "message":
                        break;
                    default:
                        throw new InvalidDataException("Received message doesn't match the pattern of header");
                }
            }

            var messageRegex = new Regex(@"^(\w+-\)[-\w\s]+\(\|){4,}message-\)(?<message>.+)\(\|$");

            if (messageRegex.IsMatch(serializedMessage))
                packet.SetMessage(messageRegex.Match(serializedMessage).Groups["message"].Value);

            return packet;
        }

        private static string MakeHeaderPeace(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }
    }
}