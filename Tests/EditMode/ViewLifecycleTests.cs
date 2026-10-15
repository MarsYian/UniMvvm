using NUnit.Framework;
using UniMvvm.Binding;
using UniMvvm.Core;
using UniMvvm.View;
using UnityEngine;

namespace UniMvvm.Tests
{
    public sealed class TestViewModel : IViewModel
    {
        public readonly ObservableProperty<int> Count = new();
    }

    [ExecuteAlways]
    public sealed class TestView : MonoBehaviour, IView<TestViewModel>
    {
        public int DisplayedCount { get; private set; }
        public int UpdateCount { get; private set; }
        public TestViewModel ViewModel { get; private set; }
        public bool IsBound { get; private set; }
        private BindingCollection _bindings;

        public void SetViewModel(TestViewModel viewModel)
        {
            if (ReferenceEquals(ViewModel, viewModel) && IsBound)
                return;

            Unbind();
            ViewModel = viewModel;
            if (viewModel != null)
                Bind();
        }

        public void Bind()
        {
            if (ViewModel == null)
                throw new System.InvalidOperationException("ViewModel is required.");

            _bindings = new BindingCollection();
            _bindings.Bind(ViewModel.Count, value =>
            {
                DisplayedCount = value;
                UpdateCount++;
            });
            IsBound = true;
        }

        public void Unbind()
        {
            IsBound = false;
            _bindings?.Dispose();
            _bindings = null;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }

    public sealed class ViewLifecycleTests
    {
        [Test]
        public void SwitchingAndDestroyingViewReleaseOldSubscriptions()
        {
            var gameObject = new GameObject("MVVM view test");
            try
            {
                var view = gameObject.AddComponent<TestView>();
                var first = new TestViewModel();
                var second = new TestViewModel();

                view.SetViewModel(first);
                view.SetViewModel(first);
                first.Count.Value = 1;
                Assert.AreEqual(2, view.UpdateCount);

                view.SetViewModel(second);
                first.Count.Value = 2;
                Assert.AreEqual(3, view.UpdateCount);
                second.Count.Value = 3;
                Assert.AreEqual(3, view.DisplayedCount);

                view.Unbind();
                second.Count.Value = 4;
                Assert.AreEqual(3, view.DisplayedCount);
                view.Bind();
                Assert.AreEqual(4, view.DisplayedCount);

                Object.DestroyImmediate(view);
                Assert.IsFalse(view.IsBound, "OnDestroy should release the binding scope.");
                int updatesAfterDestroy = view.UpdateCount;
                second.Count.Value = 5;
                Assert.AreEqual(updatesAfterDestroy, view.UpdateCount);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
