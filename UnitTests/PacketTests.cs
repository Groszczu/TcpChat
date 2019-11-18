using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core;
using Xunit;

namespace UnitTests
{
    public class PacketTests
    {
        [Fact]
        public void Packet_Should_Serialize_Correctly()
        {
            var packet = new Packet(Operation.Invite, Status.Ok, Guid.Empty,  12345, new Timestamp(new DateTime(2018, 1, 1)));
            packet.SetMessage("Accepted");
            packet.SetDestinationId(12);
            var formatter = new PacketFormatter();
            var array = formatter.Serialize(packet);
            var result = Encoding.UTF8.GetString(array);

            const string expected =
                "Identyfikator-)12345(|Operacja-)Invite(|Status-)Ok(|timestamp-)1514764800(|sid-)00000000-0000-0000-0000-000000000000(|destination-)12(|message-)Accepted(|";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Packet_Should_Deserialize_Correctly()
        {
            const string inputString =
                "sid-)10400050-5006-0070-0809-000060000000(|Operacja-)Invite(|Identyfikator-)13013(|Status-)Ok(|timestamp-)1514764800(|destination-)12(|message-)Accepted(|";
            var inputArray = Encoding.UTF8.GetBytes(inputString);
            var memStream = new MemoryStream(inputArray);
            var packet = await new PacketFormatter().DeserializeAsync(memStream);

            var expected = new Packet(Operation.Invite, Status.Ok,
                    new Guid("10400050-5006-0070-0809-000060000000"), 13013, new Timestamp(new DateTime(2018, 1, 1)))
                .SetDestinationId(12).SetMessage("Accepted");

            Assert.Equal(expected.ClientId.Value, packet.ClientId.Value);
            Assert.Equal(expected.Operation.Value, packet.Operation.Value);
            Assert.Equal(expected.Status.Value, packet.Status.Value);
            Assert.Equal(expected.Timestamp.Value, packet.Timestamp.Value);
            Assert.Equal(expected.SessionId.Value, packet.SessionId.Value);
            Assert.Equal(expected.DestinationId.Value, packet.DestinationId.Value);
            Assert.Equal(expected.Message.Value, packet.Message.Value);
        }
    }
}