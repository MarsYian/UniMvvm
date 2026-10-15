using System;
using System.Collections.ObjectModel;
using NUnit.Framework;
using UniMvvm.Binding;
using UniMvvm.Pooling;
using UniMvvm.Unity.Collections;

namespace UniMvvm.Tests
{
    public sealed class CollectionViewAdapterTests
    {
        private sealed class ItemView
        {
            public int Value;
            public int Index;
        }

        [Test]
        public void CollectionChangesKeepViewsAndPoolInSync()
        {
            var source = new ObservableCollection<int> { 1, 2 };
            using var pool = new ObjectPool<ItemView>(() => new ItemView());
            using var bindings = new BindingCollection();
            var adapter = bindings.Bind(source, pool,
                (view, item) => view.Value = item,
                view => view.Value = 0,
                (view, index) => view.Index = index);

            Assert.AreEqual(2, adapter.Count);
            source.Add(3);
            source.Move(0, 2);
            source[1] = 4;
            source.RemoveAt(0);
            Assert.AreEqual(source.Count, adapter.Count);
            Assert.AreEqual(source.Count, pool.CountActive);

            source.Clear();
            Assert.AreEqual(0, adapter.Count);
            Assert.AreEqual(0, pool.CountActive);
        }

        [Test]
        public void FailedUpdateCanRecoverAfterSourceIsFixed()
        {
            var source = new ObservableCollection<int> { 1 };
            using var pool = new ObjectPool<ItemView>(() => new ItemView());
            using var bindings = new BindingCollection();
            var adapter = bindings.Bind(source, pool,
                (view, item) =>
                {
                    if (item == 99)
                        throw new InvalidOperationException("Invalid item.");
                    view.Value = item;
                },
                view => view.Value = 0);

            Assert.Throws<InvalidOperationException>(() => source.Add(99));
            Assert.IsTrue(adapter.IsFaulted);
            Assert.IsNotNull(adapter.LastError);
            Assert.AreEqual(0, pool.CountActive);

            source.Remove(99);
            adapter.Recover();
            Assert.IsFalse(adapter.IsFaulted);
            Assert.IsNull(adapter.LastError);
            Assert.AreEqual(1, adapter.Count);
            source.Add(2);
            Assert.AreEqual(2, adapter.Count);
        }
    }
}
