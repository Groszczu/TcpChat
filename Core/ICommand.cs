using System.Net.Http;
using System.Net.Sockets;

namespace Core
{
    public interface ICommand
    {
        Packet Packet { get; set; }

        void Execute();
    }
}