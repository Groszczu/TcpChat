using Core;

namespace TCPClient.Models.Commands
{
    public class ClientDeclineInvite : ICommand
    {
        public Packet Packet { get; set; }
        public void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}