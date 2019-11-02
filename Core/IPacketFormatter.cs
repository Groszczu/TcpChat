using System.IO;
using System.Threading.Tasks;

namespace Core
{
    public interface IPacketFormatter
    {
        byte[] Serialize(Packet packet);
        Task<Packet> DeserializeAsync(Stream stream);
    }
}
