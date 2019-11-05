using Core;

namespace TCPServer.Models.Commands
{
    public class ServerDeclineInvite : ICommand
    {
        public Packet Packet { get; set; }
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}