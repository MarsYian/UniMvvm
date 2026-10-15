using UniMvvm.Core;
using System;

namespace UniMvvm.Binding
{
    /// <summary>Synchronizes a property with a view event, without an event feedback loop.</summary>
    public sealed class TwoWayPropertyBinding<T> : IBinding
    {
        private readonly IObservableProperty<T> _source;
        private readonly Action<T> _updateView;
        private IDisposable _sourceSubscription;
        private IDisposable _viewSubscription;
        private bool _updatingView;
        private bool _disposed;

        public TwoWayPropertyBinding(IObservableProperty<T> source, Action<T> updateView, Func<Action<T>, IDisposable> subscribeView)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _updateView = updateView ?? throw new ArgumentNullException(nameof(updateView));
            if (subscribeView == null)
                throw new ArgumentNullException(nameof(subscribeView));

            try
            {
                _sourceSubscription = _source.Subscribe(ApplySourceValue);
                _viewSubscription = subscribeView(OnViewValueChanged);
                if (_viewSubscription == null)
                    throw new InvalidOperationException("subscribeView must return an IDisposable unsubscriber.");
            }
            catch
            {
                _viewSubscription?.Dispose();
                _sourceSubscription?.Dispose();
                throw;
            }
        }

        private void ApplySourceValue(T value)
        {
            if (_disposed)
                return;

            _updatingView = true;
            try
            {
                _updateView(value);
            }
            finally
            {
                _updatingView = false;
            }
        }

        private void OnViewValueChanged(T value)
        {
            if (!_disposed && !_updatingView)
                _source.Value = value;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            try
            {
                _viewSubscription?.Dispose();
            }
            finally
            {
                _viewSubscription = null;
                _sourceSubscription?.Dispose();
                _sourceSubscription = null;
            }
        }
    }
}
