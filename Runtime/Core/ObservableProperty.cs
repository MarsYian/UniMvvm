using System;
using System.Collections.Generic;

namespace UniMvvm.Core
{
    /// <summary>Value change notifications. Use on the Unity main thread.</summary>
    public sealed class ObservableProperty<T> : IObservableProperty<T>
    {
        private T _value;
        private event Action<T> _changed;

        public ObservableProperty(T initialValue = default)
        {
            _value = initialValue;
        }

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value))
                    return;

                _value = value;
                _changed?.Invoke(value);
            }
        }

        public IDisposable Subscribe(Action<T> observer, bool notifyImmediately = true)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            _changed += observer;
            try
            {
                if (notifyImmediately)
                    observer(_value);
            }
            catch
            {
                _changed -= observer;
                throw;
            }

            return new Subscription(this, observer);
        }

        private sealed class Subscription : IDisposable
        {
            private ObservableProperty<T> _owner;
            private Action<T> _observer;

            public Subscription(ObservableProperty<T> owner, Action<T> observer)
            {
                _owner = owner;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_owner == null)
                    return;

                _owner._changed -= _observer;
                _owner = null;
                _observer = null;
            }
        }
    }
}
