using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Core;
using Xunit;

namespace UnitTests
{
    public class PacketTests
    {
        [Fact]
        public void Packet_Should_Serialize_Correctly()
        {
            var packet = new Packet(Operation.AcceptInvite, Status.Ok, Guid.Empty, "Accepted");

            var formatter = new PacketFormatter();
            var array = formatter.Serialize(packet);
            var result = Encoding.UTF8.GetString(array);

            const string expected = "operation-)AcceptInvite(|status-)Ok(|id-)00000000-0000-0000-0000-000000000000(| Accepted";
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Packet_Should_Deserialize_Correctly()
        {
            var inputString = "operation-)Invite(|status-)Ok(|id-)00000000-0000-0000-0000-000000000000(| 134";
            var inputArray = Encoding.UTF8.GetBytes(inputString);
            var packet = new PacketFormatter().Deserialize(inputArray);

            var expected = new Packet(Operation.Invite, Status.Ok, Guid.Empty, "134");
            Assert.Equal(expected.Operation, packet.Operation);
            Assert.Equal(expected.Status, packet.Status);
            Assert.Equal(expected.Id, packet.Id);
            Assert.Equal(expected.Message, packet.Message);
        }
    }
}