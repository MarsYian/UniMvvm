using System;
using System.Collections.Generic;
using UniMvvm.Binding;
using UniMvvm.Pooling;

namespace UniMvvm.Unity.Collections
{
    public static class CollectionBindingExtensions
    {
        public static CollectionViewAdapter<TItem, TView> Bind<TItem, TView>(this BindingCollection bindings,
            IReadOnlyList<TItem> source,
            IObjectPool<TView> pool,
            Action<TView, TItem> bindView,
            Action<TView> unbindView,
            Action<TView, int> arrangeView = null) where TView : class
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            var adapter = new CollectionViewAdapter<TItem, TView>(source, pool, bindView, unbindView, arrangeView);
            return bindings.Add(adapter);
        }
    }
}
