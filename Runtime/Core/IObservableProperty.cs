namespace UniMvvm.Core
{
    public interface IObservableProperty<T> : IReadOnlyObservableProperty<T>
    {
        new T Value { get; set; }
    }
}
