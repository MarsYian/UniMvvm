using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UniMvvm.Binding
{
    public sealed class CollectionBinding<T> : IBinding
    {
        private readonly INotifyCollectionChanged _source;
        private readonly Action<NotifyCollectionChangedEventArgs> _onChanged;
        private bool _disposed;

        public CollectionBinding(IReadOnlyList<T> source, Action<IReadOnlyList<T>> initialize, Action<NotifyCollectionChangedEventArgs> onChanged)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is not INotifyCollectionChanged observable)
                throw new ArgumentException("Source must implement INotifyCollectionChanged.", nameof(source));

            if (initialize == null)
                throw new ArgumentNullException(nameof(initialize));

            _onChanged = onChanged ?? throw new ArgumentNullException(nameof(onChanged));
            _source = observable;

            _source.CollectionChanged += OnCollectionChanged;

            try
            {
                initialize(source);
            }
            catch
            {
                _source.CollectionChanged -= OnCollectionChanged;
                throw;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_disposed)
                _onChanged(e);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _source.CollectionChanged -= OnCollectionChanged;
        }
    }
}