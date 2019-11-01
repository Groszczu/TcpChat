using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Core;
using Xunit;

namespace UnitTests
{
    public class PacketTests
    {
        [Fact]
        public void Packet_Should_Serialize_Correctly()
        {
            var packet = new Packet(Operation.AcceptInvite, Status.Ok, 12345, "Accepted");
            var mem = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(mem, packet);

            var arr = mem.ToArray();
            string StringByte= BitConverter.ToString(arr);
        }
    }
}
