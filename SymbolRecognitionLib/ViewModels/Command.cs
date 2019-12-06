using System;
using System.Windows.Input;

namespace SymbolRecognitionLib.ViewModels
{
    public class Command : ICommand
    {
        Action<object> handler;
        public event EventHandler CanExecuteChanged;

        public Command(Action<object> executeHandler)
        {
            handler = executeHandler;
        }

        public bool CanExecute(object parameter)
        {
            // Always can be executed
            return true;
        }

        public void Execute(object parameter)
        {
            handler(parameter);
        }
    }
}
