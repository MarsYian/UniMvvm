using System;
using TMPro;
using UniMvvm.Binding;
using UniMvvm.Core;
using UniMvvm.Unity.Events;
using UnityEngine;

namespace UniMvvm.Unity.Tmp
{
    /// <summary>Bindings for TextMeshPro controls. This assembly is compiled only when TextMeshPro is installed.</summary>
    public static class TmpBindingExtensions
    {
        public static IBinding BindColor(this BindingCollection bindings, IReadOnlyObservableProperty<Color> source, TMP_Text target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.color = value);
        }

        public static IBinding BindText(this BindingCollection bindings, IReadOnlyObservableProperty<string> source, TMP_Text target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.text = value ?? string.Empty);
        }

        public static IBinding BindInputField(this BindingCollection bindings, IObservableProperty<string> source, TMP_InputField target)
        {
            RequireTarget(target);
            return bindings.Bind(source, value => target.SetTextWithoutNotify(value ?? string.Empty), target.onValueChanged);
        }

        public static IBinding BindDropdown(this BindingCollection bindings, IObservableProperty<int> source, TMP_Dropdown target)
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