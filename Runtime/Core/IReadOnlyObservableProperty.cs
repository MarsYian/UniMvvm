using System;

namespace UniMvvm.Core
{
    public interface IReadOnlyObservableProperty<T>
    {
        T Value { get; }
        IDisposable Subscribe(Action<T> observer, bool notifyImmediately = true);
    }
}
