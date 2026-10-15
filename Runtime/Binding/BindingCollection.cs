using UniMvvm.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UniMvvm.Binding
{
    /// <summary>Owns all active bindings for a single View. Bind calls activate immediately.</summary>
    public sealed class BindingCollection : IDisposable
    {
        private readonly List<IBinding> _bindings = new List<IBinding>(8);
        private bool _disposed;

        public int Count => _bindings.Count;

        public IBinding Bind<T>(IReadOnlyObservableProperty<T> source, Action<T> updateView)
        {
            ThrowIfDisposed();
            return Track(new PropertyBinding<T>(source, updateView));
        }

        public IBinding Bind<T>(IObservableProperty<T> source, Action<T> updateView, Func<Action<T>, IDisposable> subscribeView)
        {
            ThrowIfDisposed();
            return Track(new TwoWayPropertyBinding<T>(source, updateView, subscribeView));
        }

        public IBinding Bind<T>(IReadOnlyList<T> source, Action<IReadOnlyList<T>> initialize, Action<NotifyCollectionChangedEventArgs> onChanged)
        {
            ThrowIfDisposed();
            return Track(new CollectionBinding<T>(source, initialize, onChanged));
        }

        public IBinding Bind(ICommand command, Func<Action, IDisposable> subscribeEvent, Action<bool> updateEnabled = null)
        {
            ThrowIfDisposed();
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            Func<Action, IDisposable> subscribeChanged = changed =>
            {
                command.CanExecuteChanged += changed;
                return new ActionSubscription(() => command.CanExecuteChanged -= changed);
            };

            return Track(new CommandBinding(() => command.CanExecute, command.Execute, subscribeEvent, subscribeChanged, updateEnabled));
        }

        public IBinding Bind<T>(ICommand<T> command, T parameter, Func<Action, IDisposable> subscribeEvent, Action<bool> updateEnabled = null)
        {
            ThrowIfDisposed();
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            Func<Action, IDisposable> subscribeChanged = changed =>
            {
                command.CanExecuteChanged += changed;
                return new ActionSubscription(() => command.CanExecuteChanged -= changed);
            };

            return Track(new CommandBinding(() => command.CanExecute(parameter), () => command.Execute(parameter), subscribeEvent, subscribeChanged, updateEnabled));
        }

        public TBinding Add<TBinding>(TBinding binding) where TBinding : IBinding
        {
            if (binding == null)
                throw new ArgumentNullException(nameof(binding));

            try
            {
                ThrowIfDisposed();
                _bindings.Add(binding);
                return binding;
            }
            catch
            {
                binding.Dispose();
                throw;
            }
        }

        private IBinding Track(IBinding binding)
        {
            try
            {
                _bindings.Add(binding);
                return binding;
            }
            catch
            {
                binding.Dispose();
                throw;
            }
        }

        public void Clear()
        {
            ThrowIfDisposed();
            ReleaseAll();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            ReleaseAll();
        }

        private void ReleaseAll()
        {
            List<Exception> errors = null;
            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                try
                {
                    _bindings[i].Dispose();
                }
                catch (Exception ex)
                {
                    if (errors == null)
                        errors = new List<Exception>();
                    errors.Add(ex);
                }
            }

            _bindings.Clear();
            if (errors != null)
                throw new AggregateException("One or more bindings failed to release.", errors);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BindingCollection));
        }
    }
}