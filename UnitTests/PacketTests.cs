using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
            var packet = new Packet(Operation.AcceptInvite, Status.Ok, Guid.Empty, "Accepted");

            var formatter = new PacketFormatter();
            var array = formatter.Serialize(packet);
            var result = Encoding.UTF8.GetString(array);

            const string expected = "operation-)AcceptInvite(|status-)Ok(|id-)00000000-0000-0000-0000-000000000000(| Accepted";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Packet_Should_Deserialize_Correctly()
        {
            var inputString = "id-)10400050-5006-0070-0809-000060000000(|operation-)1(|status-)1(|length-)   12(|message-)Hello world!(|";
            var inputArray = Encoding.UTF8.GetBytes(inputString);
            var memStream = new MemoryStream(inputArray);
            var packet = await new PacketFormatter().DeserializeAsync(memStream);

            var expected = new Packet(Operation.AcceptInvite, Status.Unauthorized, new Guid("10400050-5006-0070-0809-000060000000"), "Hello world!");
            Assert.Equal(expected.Id, packet.Id);
            Assert.Equal(expected.Operation, packet.Operation);
            Assert.Equal(expected.Status, packet.Status);
            Assert.Equal(expected.MessageLength, packet.MessageLength);
            Assert.Equal(expected.Message, packet.Message);
        }
    }
}