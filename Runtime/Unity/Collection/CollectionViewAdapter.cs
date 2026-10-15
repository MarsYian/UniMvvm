using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UniMvvm.Binding;
using UniMvvm.Pooling;

namespace UniMvvm.Unity.Collections
{
    public sealed class CollectionViewAdapter<TItem, TView> : IBinding where TView : class
    {
        private readonly IReadOnlyList<TItem> _source;
        private readonly IObjectPool<TView> _pool;
        private readonly Action<TView, TItem> _bindView;
        private readonly Action<TView> _unbindView;
        private readonly Action<TView, int> _arrangeView;
        private readonly List<TView> _views = new();

        private IDisposable _subscription;
        private bool _disposed;
        private bool _updating;
        private Exception _lastError;

        public int Count => _views.Count;
        public bool IsFaulted => _lastError != null;
        public Exception LastError => _lastError;

        public CollectionViewAdapter(IReadOnlyList<TItem> source,
            IObjectPool<TView> pool,
            Action<TView, TItem> bindView,
            Action<TView> unbindView,
            Action<TView, int> arrangeView = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
            _bindView = bindView ?? throw new ArgumentNullException(nameof(bindView));
            _unbindView = unbindView ?? throw new ArgumentNullException(nameof(unbindView));
            _arrangeView = arrangeView;

            try
            {
                _subscription = new CollectionBinding<TItem>(_source, _ => Initialize(), OnChanged);
            }
            catch (Exception error)
            {
                _disposed = true;

                try
                {
                    ReleaseAll();
                }
                catch (Exception cleanupError)
                {
                    throw new AggregateException("Collection initialization and cleanup failed.", error, cleanupError);
                }

                throw;
            }
        }

        private void Initialize()
        {
            _updating = true;

            try
            {
                Rebuild();
            }
            finally
            {
                _updating = false;
            }
        }

        private void OnChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_disposed)
                return;

            if (_updating)
                throw new InvalidOperationException("Reentrant collection updates are not supported.");

            _updating = true;

            try
            {
                ApplyChange(e);

                if (_views.Count != _source.Count)
                    throw new InvalidOperationException("Collection and View counts are inconsistent.");
            }
            catch (Exception error)
            {
                var failure = EnterFault(error);
                if (!ReferenceEquals(failure, error))
                    throw failure;
                throw;
            }
            finally
            {
                _updating = false;
            }
        }

        /// <summary>Rebuilds all views and resumes collection notifications after a failed update.</summary>
        public void Recover()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CollectionViewAdapter<TItem, TView>));
            if (!IsFaulted)
                return;

            try
            {
                _subscription = new CollectionBinding<TItem>(_source, _ => Initialize(), OnChanged);
                _lastError = null;
            }
            catch (Exception error)
            {
                throw EnterFault(error);
            }
        }

        private Exception EnterFault(Exception error)
        {
            List<Exception> errors = null;
            try
            {
                _subscription?.Dispose();
            }
            catch (Exception cleanupError)
            {
                errors = new List<Exception> { error, cleanupError };
            }

            _subscription = null;
            try
            {
                ReleaseAll();
            }
            catch (Exception cleanupError)
            {
                (errors ??= new List<Exception> { error }).Add(cleanupError);
            }

            _lastError = errors == null
                ? error
                : new AggregateException("Collection update and cleanup failed.", errors);
            return _lastError;
        }

        private void ApplyChange(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null || e.NewStartingIndex < 0 || e.NewStartingIndex > _views.Count)
                    {
                        Rebuild();
                        return;
                    }

                    InsertRange(e.NewStartingIndex, e.NewItems);
                    Arrange(e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null || e.OldStartingIndex < 0)
                    {
                        Rebuild();
                        return;
                    }

                    RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    Arrange(e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems == null || e.NewItems == null || e.OldStartingIndex < 0 || e.OldStartingIndex != e.NewStartingIndex)
                    {
                        Rebuild();
                        return;
                    }

                    RemoveRange(e.OldStartingIndex, e.OldItems.Count);
                    InsertRange(e.OldStartingIndex, e.NewItems);
                    Arrange(e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Move:
                    MoveRange(e);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    Rebuild();
                    break;

                default:
                    Rebuild();
                    break;
            }
        }

        private void MoveRange(NotifyCollectionChangedEventArgs e)
        {
            int count = e.OldItems?.Count ?? 0;

            if (count <= 0 || e.OldStartingIndex < 0 || e.NewStartingIndex < 0)
            {
                Rebuild();
                return;
            }

            var moved = _views.GetRange(e.OldStartingIndex, count);

            _views.RemoveRange(e.OldStartingIndex, count);
            _views.InsertRange(e.NewStartingIndex, moved);

            Arrange(Math.Min(e.OldStartingIndex, e.NewStartingIndex));
        }

        private void InsertRange(int index, IList items)
        {
            for (int i = 0; i < items.Count; i++)
                Insert(index + i, (TItem)items[i]);
        }

        private void Insert(int index, TItem item)
        {
            TView view = _pool.Get();

            try
            {
                _bindView(view, item);
                _views.Insert(index, view);
            }
            catch (Exception error)
            {
                var errors = new List<Exception> { error };

                try
                {
                    _unbindView(view);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                try
                {
                    _pool.Release(view);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }

                if (errors.Count > 1)
                    throw new AggregateException("Failed to insert and clean up a collection view.", errors);

                throw;
            }
        }

        private void RemoveRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                TView view = _views[index];
                _views.RemoveAt(index);

                try
                {
                    _unbindView(view);
                }
                finally
                {
                    _pool.Release(view);
                }
            }
        }

        private void Arrange(int startIndex)
        {
            if (_arrangeView == null)
                return;

            for (int i = Math.Max(0, startIndex); i < _views.Count; i++)
                _arrangeView(_views[i], i);
        }

        private void Rebuild()
        {
            ReleaseAll();

            for (int i = 0; i < _source.Count; i++)
                Insert(i, _source[i]);

            Arrange(0);
        }

        private void ReleaseAll()
        {
            List<Exception> errors = null;

            for (int i = _views.Count - 1; i >= 0; i--)
            {
                TView view = _views[i];
                _views.RemoveAt(i);

                try
                {
                    _unbindView(view);
                }
                catch (Exception ex)
                {
                    (errors ??= new List<Exception>()).Add(ex);
                }

                try
                {
                    _pool.Release(view);
                }
                catch (Exception ex)
                {
                    (errors ??= new List<Exception>()).Add(ex);
                }
            }

            if (errors != null)
                throw new AggregateException("Failed to release collection views.", errors);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            List<Exception> errors = null;

            try
            {
                _subscription?.Dispose();
            }
            catch (Exception ex)
            {
                (errors ??= new List<Exception>()).Add(ex);
            }

            _subscription = null;

            try
            {
                ReleaseAll();
            }
            catch (Exception ex)
            {
                (errors ??= new List<Exception>()).Add(ex);
            }

            if (errors != null)
                throw new AggregateException("Failed to dispose collection adapter.", errors);
        }
    }
}
