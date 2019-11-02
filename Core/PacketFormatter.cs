using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core
{
    public class PacketFormatter: IPacketFormatter
    {
        public byte[] Serialize(Packet packet)
        {
            var str = MakeHeaderPeace("operation", packet.Operation.ToString("g"))
                   + MakeHeaderPeace("status", packet.Status.ToString("g"))
                   + MakeHeaderPeace("id", packet.Id.ToString())
                   + " " + packet.Message;

            return Encoding.UTF8.GetBytes(str);
        }

        public Packet Deserialize(byte[] array)
        {
            var message = Encoding.UTF8.GetString(array);
            var regex = new Regex(@"(?<key>\w+)-\)(?<value>\w+)\(\|");
            if (!regex.IsMatch(message))
                throw new InvalidDataException("Received message doesn't match the pattern of header");

            var data = new Packet();
            
            foreach (Match match in regex.Matches(message))
            {
                switch (match.Groups["key"].Value)
                {
                    case "operation": Enum.TryParse(match.Groups["value"].Value, out Operation opr);
                        data.Operation = opr;
                        break;
                    case "status": Enum.TryParse(match.Groups["value"].Value, out Status status);
                        data.Status = status;
                        break;
                    case "id": data.Id = Guid.Parse(match.Groups["value"].Value);
                        break;
                    default:
                        throw new InvalidDataException("Received message doesn't match the pattern of header");
                }
            }

            regex = new Regex(@"(\w+-\)(\w|-)+\(\|){3}\s*(?<message>.*)");
            data.Message = !regex.IsMatch(message) ? "Empty message" : regex.Match(message).Groups["message"].Value;

            return data;
        }
        private string MakeHeaderPeace(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }
    } 
}
