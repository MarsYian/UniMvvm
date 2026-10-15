using UniMvvm.Core;
using System;

namespace UniMvvm.Binding
{
    public sealed class PropertyBinding<T> : IBinding
    {
        private IDisposable _subscription;

        public PropertyBinding(IReadOnlyObservableProperty<T> source, Action<T> updateView)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (updateView == null)
                throw new ArgumentNullException(nameof(updateView));

            // Subscribe pushes the initial value by default.
            _subscription = source.Subscribe(updateView);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}
