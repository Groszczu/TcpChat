using System.IO;

namespace Core
{
    public interface ISender
    {
        void Send(byte[] bytes);
    }
}