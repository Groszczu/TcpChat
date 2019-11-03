using System.Windows.Input;

namespace Core
{
    public interface ICommandHandler
    {
        void Handle(ICommand command);
    }
}