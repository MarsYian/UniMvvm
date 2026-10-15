using System;

namespace UniMvvm.Core
{
    public sealed class RelayCommand<T> : ICommand<T>
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event Action CanExecuteChanged;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(T parameter)
        {
            if (CanExecute(parameter))
                _execute(parameter);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke();
        }
    }
}
