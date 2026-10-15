using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UniMvvm.Pooling
{
    public sealed class ObjectPool<T> : IObjectPool<T>, IDisposable where T : class
    {
        private readonly Queue<T> _available = new();
        private readonly HashSet<T> _active = new(ReferenceComparer.Instance);

        private readonly Func<T> _create;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;
        private readonly int _maxInactive;

        private bool _disposed;

        public int CountActive => _active.Count;
        public int CountInactive => _available.Count;

        public ObjectPool(Func<T> create, Action<T> onGet = null, Action<T> onRelease = null, Action<T> onDestroy = null, int maxInactive = 64)
        {
            _create = create ?? throw new ArgumentNullException(nameof(create));
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;

            if (maxInactive < 0)
                throw new ArgumentOutOfRangeException(nameof(maxInactive));

            _maxInactive = maxInactive;
        }

        public T Get()
        {
            ThrowIfDisposed();

            T instance = _available.Count > 0 ? _available.Dequeue() : _create();

            if (instance == null)
                throw new InvalidOperationException("Pool factory returned null.");

            if (!_active.Add(instance))
                throw new InvalidOperationException("Pool factory returned an object already in use.");

            try
            {
                _onGet?.Invoke(instance);
                return instance;
            }
            catch
            {
                _active.Remove(instance);
                _onDestroy?.Invoke(instance);
                throw;
            }
        }

        public void Release(T instance)
        {
            ThrowIfDisposed();

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!_active.Remove(instance))
                throw new InvalidOperationException("Object does not belong to the active pool.");

            try
            {
                _onRelease?.Invoke(instance);
            }
            catch
            {
                _onDestroy?.Invoke(instance);
                throw;
            }

            if (_available.Count < _maxInactive)
                _available.Enqueue(instance);
            else
                _onDestroy?.Invoke(instance);
        }

        public void Clear()
        {
            ThrowIfDisposed();
            ClearInactive();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_active.Count > 0)
                throw new InvalidOperationException("Release all active objects before disposing the pool.");

            _disposed = true;
            ClearInactive();
        }

        private void ClearInactive()
        {
            List<Exception> errors = null;

            while (_available.Count > 0)
            {
                T instance = _available.Dequeue();

                try
                {
                    _onDestroy?.Invoke(instance);
                }
                catch (Exception ex)
                {
                    (errors ??= new List<Exception>()).Add(ex);
                }
            }

            if (errors != null)
                throw new AggregateException("Failed to destroy pooled objects.", errors);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ObjectPool<T>));
        }

        private sealed class ReferenceComparer : IEqualityComparer<T>
        {
            public static readonly ReferenceComparer Instance = new();

            public bool Equals(T x, T y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}