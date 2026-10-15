using System;
using UniMvvm.Binding;
using UniMvvm.Core;
using UniMvvm.View;
using UnityEngine;

namespace UniMvvm.Samples.Integration
{
    public abstract class UIViewBase<TViewModel> : MonoBehaviour, IView<TViewModel>
        where TViewModel : class, IViewModel
    {
        protected BindingCollection Bindings { get; private set; }

        public TViewModel ViewModel { get; private set; }

        public void SetViewModel(TViewModel viewModel)
        {
            if (ReferenceEquals(ViewModel, viewModel) && Bindings != null)
                return;

            Unbind();
            ViewModel = viewModel;
            if (viewModel != null)
                Bind();
        }

        public abstract void Bind();

        public virtual void Unbind()
        {
            Bindings?.Dispose();
            Bindings = null;
        }

        protected BindingCollection BeginBind()
        {
            if (ViewModel == null)
                throw new InvalidOperationException("Set a ViewModel before binding the View.");
            if (Bindings != null)
                throw new InvalidOperationException("Unbind before creating a new binding collection.");

            Bindings = new BindingCollection();
            return Bindings;
        }

        protected virtual void OnDestroy()
        {
            Unbind();
        }
    }
}
