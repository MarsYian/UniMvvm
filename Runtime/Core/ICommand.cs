using System;

namespace UniMvvm.Core
{
    public interface ICommand
    {
        bool CanExecute { get; }
        event Action CanExecuteChanged;
        void Execute();
    }

    public interface ICommand<T>
    {
        event Action CanExecuteChanged;
        bool CanExecute(T parameter);
        void Execute(T parameter);
    }
}
