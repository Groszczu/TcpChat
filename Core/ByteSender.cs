using System.IO;
using System.Net.Sockets;

namespace Core
{
    public class ByteSender : ISender
    {
        private readonly NetworkStream _stream;

        public ByteSender(NetworkStream stream)
        {
            _stream = stream;
        }
        
        public void Send(byte[] bytes)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }
    }
}