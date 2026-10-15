using System;

namespace UniMvvm.Binding
{
    /// <summary>Binds an event to command execution and optionally reflects CanExecute in the view.</summary>
    public sealed class CommandBinding : IBinding
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        private readonly Action<bool> _updateEnabled;
        private IDisposable _eventSubscription;
        private IDisposable _conditionSubscription;
        private bool _disposed;

        public CommandBinding(Func<bool> canExecute, Action execute, Func<Action, IDisposable> subscribeEvent, Func<Action, IDisposable> subscribeCanExecuteChanged, Action<bool> updateEnabled = null)
        {
            _canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _updateEnabled = updateEnabled;
            if (subscribeEvent == null)
                throw new ArgumentNullException(nameof(subscribeEvent));
            if (subscribeCanExecuteChanged == null)
                throw new ArgumentNullException(nameof(subscribeCanExecuteChanged));

            try
            {
                _eventSubscription = subscribeEvent(OnInvoked);
                if (_eventSubscription == null)
                    throw new InvalidOperationException("subscribeEvent must return an IDisposable unsubscriber.");

                _conditionSubscription = subscribeCanExecuteChanged(Refresh);
                if (_conditionSubscription == null)
                    throw new InvalidOperationException("subscribeCanExecuteChanged must return an IDisposable unsubscriber.");

                Refresh();
            }
            catch
            {
                try { _conditionSubscription?.Dispose(); }
                finally { _eventSubscription?.Dispose(); }
                throw;
            }
        }

        private void OnInvoked()
        {
            if (!_disposed && _canExecute())
                _execute();
        }

        private void Refresh()
        {
            if (!_disposed)
                _updateEnabled?.Invoke(_canExecute());
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            try
            {
                _conditionSubscription?.Dispose();
            }
            finally
            {
                _conditionSubscription = null;
                _eventSubscription?.Dispose();
                _eventSubscription = null;
            }
        }
    }
}
