using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Core;

namespace TCPServer
{
    public static class HeaderBuilder
    {
        public static string ConvertPacketToString(PacketData data)
        {
            return MakeHeaderPeace("operation", data.Operation.ToString("g"))
                   + MakeHeaderPeace("status", data.Status.ToString("g"))
                   + MakeHeaderPeace("id", data.Id.ToString())
                   + " " + data.Message;
        }

        private static string MakeHeaderPeace(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }
        public static PacketData ConvertStringToPacket(string message)
        {
            var regex = new Regex(@"(?<key>\w+)\-\)(?<value>\w+)\(\|");
            if (!regex.IsMatch(message))
                throw new InvalidDataException("Received message doesn't match the pattern of header");

            var data = new PacketData();
            
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
                    case "id": data.Id = int.Parse(match.Groups["value"].Value);
                        break;
                    default:
                        throw new InvalidDataException("Received message doesn't match the pattern of header");
                }
            }

            regex = new Regex(@"(\w+\-\)\w+\(\|){3}\s*(?<message>.*)");
            data.Message = !regex.IsMatch(message) ? "Empty message" : regex.Match(message).Groups["message"].Value;

            return data;
        }
    }

    public class PacketData
    {
        public Operation Operation { get; set; }
        public Status Status { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }

        public PacketData(Operation operation, Status status, int id, string message)
        {
            Operation = operation;
            Status = status;
            Id = id;
            Message = message;
        }

        public PacketData()
        {
            
        }
    }
}
