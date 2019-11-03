namespace Core
{
    public class OperationsHandler : ICommandHandler
    {
        public void Handle(ICommand command)
        {
            command.Execute();
        }
    }
}