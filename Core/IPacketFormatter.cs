
namespace Core
{
    public interface IPacketFormatter
    {
        byte[] Serialize(Packet packet);
        Packet Deserialize(byte[] array);
    }
}
