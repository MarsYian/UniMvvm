using System;

namespace UniMvvm.Core
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event Action CanExecuteChanged;
        public bool CanExecute => _canExecute?.Invoke() ?? true;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public void Execute()
        {
            if (CanExecute)
                _execute();
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }
}
