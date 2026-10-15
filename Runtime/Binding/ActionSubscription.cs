using System;

namespace UniMvvm.Binding
{
    /// <summary>A lightweight way to express how to unsubscribe from an external event.</summary>
    public sealed class ActionSubscription : IDisposable
    {
        private Action _unsubscribe;

        public ActionSubscription(Action unsubscribe)
        {
            _unsubscribe = unsubscribe ?? throw new ArgumentNullException(nameof(unsubscribe));
        }

        public void Dispose()
        {
            var action = _unsubscribe;
            _unsubscribe = null;
            action?.Invoke();
        }
    }
}
