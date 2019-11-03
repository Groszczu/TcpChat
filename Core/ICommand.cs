namespace Core
{
    public interface ICommand
    {
        Packet Packet { get; set; }

        void Execute();
    }
}