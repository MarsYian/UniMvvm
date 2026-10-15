using System;
using UnityEngine.Events;
using UniMvvm.Binding;
using UniMvvm.Core;

namespace UniMvvm.Unity.Events
{
    /// <summary>Generic UnityEvent adapters. No dependency on Button, Text, Slider, or TMP types.</summary>
    public static class UnityEventBindingExtensions
    {
        public static IBinding Bind<T>(this BindingCollection bindings, IObservableProperty<T> source, Action<T> updateView, UnityEvent<T> viewChanged)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (viewChanged == null)
                throw new ArgumentNullException(nameof(viewChanged));

            return bindings.Bind(source, updateView, callback =>
            {
                UnityAction<T> listener = callback.Invoke;
                viewChanged.AddListener(listener);
                return new ActionSubscription(() => viewChanged.RemoveListener(listener));
            });
        }

        public static IBinding Bind(this BindingCollection bindings, UnityEvent viewEvent, ICommand command, Action<bool> updateEnabled = null)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (viewEvent == null)
                throw new ArgumentNullException(nameof(viewEvent));

            return bindings.Bind(command, callback =>
            {
                UnityAction listener = callback.Invoke;
                viewEvent.AddListener(listener);
                return new ActionSubscription(() => viewEvent.RemoveListener(listener));
            }, updateEnabled);
        }

        public static IBinding Bind<T>(this BindingCollection bindings, UnityEvent viewEvent, ICommand<T> command, T parameter, Action<bool> updateEnabled = null)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (viewEvent == null)
                throw new ArgumentNullException(nameof(viewEvent));

            return bindings.Bind(command, parameter, callback =>
            {
                UnityAction listener = callback.Invoke;
                viewEvent.AddListener(listener);
                return new ActionSubscription(() => viewEvent.RemoveListener(listener));
            }, updateEnabled);
        }
    }
}
