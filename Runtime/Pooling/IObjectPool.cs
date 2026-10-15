namespace UniMvvm.Pooling
{
    public interface IObjectPool<T>
    {
        T Get();
        void Release(T instance);
    }
}