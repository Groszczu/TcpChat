using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Core;
using Xunit;

namespace UnitTests
{
   /* public class PacketTests
    {
        [Fact]
        public void Packet_Should_Serialize_Correctly()
        {
            var packet = new Packet(Operation.Invite, Status.Ok, Guid.Empty, new Timestamp(new DateTime(2018, 1, 1)));
            packet.SetMessage("Accepted");
            packet.SetDestinationId(12);
            var formatter = new PacketFormatter();
            var array = formatter.Serialize(packet);
            var result = Encoding.UTF8.GetString(array);

            const string expected =
                "id-)00000000-0000-0000-0000-000000000000(|operation-)Invite(|status-)Ok(|timestamp-)1514764800(|destination-)12(|length-)8(|message-)Accepted(|";
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Packet_Should_Deserialize_Correctly()
        {
            const string inputString =
                "id-)10400050-5006-0070-0809-000060000000(|operation-)Invite(|status-)Ok(|timestamp-)1514764800(|destination-)12(|length-)8(|message-)Accepted(|";
            var inputArray = Encoding.UTF8.GetBytes(inputString);
            var memStream = new MemoryStream(inputArray);
            var packet = await new PacketFormatter().DeserializeAsync(memStream);

            var expected = new Packet(Operation.Invite, Status.Ok,
                new Guid("10400050-5006-0070-0809-000060000000"), new Timestamp(new DateTime(2018, 1, 1))).SetDestinationId(12).SetMessage("Accepted");
                
            Assert.Equal(expected.Id.ObjectValue, packet.Id.ObjectValue);
            Assert.Equal(expected.Operation.ObjectValue, packet.Operation.ObjectValue);
            Assert.Equal(expected.Status.ObjectValue, packet.Status.ObjectValue);
            Assert.Equal(expected.Message.ObjectValue, packet.Message.ObjectValue);
        }
    }*/
}