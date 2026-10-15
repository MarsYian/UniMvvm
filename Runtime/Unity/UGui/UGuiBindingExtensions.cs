using System;
using UniMvvm.Binding;
using UniMvvm.Core;
using UniMvvm.Unity.Events;
using UnityEngine.UI;

namespace UniMvvm.Unity.UGui
{
    /// <summary>Bindings for commonly used uGUI controls.</summary>
    public static class UGuiBindingExtensions
    {
        public static IBinding BindText(this BindingCollection bindings, IReadOnlyObservableProperty<string> source, Text target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.text = value ?? string.Empty);
        }

        public static IBinding BindInteractable(this BindingCollection bindings, IReadOnlyObservableProperty<bool> source, Selectable target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.interactable = value);
        }

        public static IBinding BindCommand(this BindingCollection bindings, ICommand command, Button target)
        {
            RequireTarget(target);
            return bindings.Bind(target.onClick, command, value => target.interactable = value);
        }

        public static IBinding BindToggle(this BindingCollection bindings, IObservableProperty<bool> source, Toggle target)
        {
            RequireTarget(target);
            return bindings.Bind(source, target.SetIsOnWithoutNotify, target.onValueChanged);
        }

        public static IBinding BindSlider(this BindingCollection bindings, IObservableProperty<float> source, Slider target)
        {
            RequireTarget(target);
            return bindings.Bind(source, target.SetValueWithoutNotify, target.onValueChanged);
        }

        public static IBinding BindInputField(this BindingCollection bindings, IObservableProperty<string> source, InputField target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.SetTextWithoutNotify(value ?? string.Empty), target.onValueChanged);
        }

        public static IBinding BindDropdown(this BindingCollection bindings, IObservableProperty<int> source, Dropdown target)
        {
            RequireTarget(target);
            return bindings.Bind(source, target.SetValueWithoutNotify, target.onValueChanged);
        }

        private static void RequireTarget(UnityEngine.Object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
        }
    }
}
