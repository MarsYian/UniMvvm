using NUnit.Framework;
using UniMvvm.Binding;
using UniMvvm.Core;
using UniMvvm.Unity.UGui;
using UnityEngine;
using UnityEngine.UI;

namespace UniMvvm.Tests
{
    public sealed class UGuiBindingExtensionsTests
    {
        [Test]
        public void TwoWayControlsAndCommandReleaseWithBindingCollection()
        {
            var toggleObject = new GameObject("Toggle binding test");
            var sliderObject = new GameObject("Slider binding test");
            var buttonObject = new GameObject("Button binding test");
            try
            {
                var toggle = toggleObject.AddComponent<Toggle>();
                var slider = sliderObject.AddComponent<Slider>();
                var button = buttonObject.AddComponent<Button>();
                var enabled = new ObservableProperty<bool>(true);
                var volume = new ObservableProperty<float>(0.25f);
                var canExecute = true;
                var executionCount = 0;
                var command = new RelayCommand(() => executionCount++, () => canExecute);

                using var bindings = new BindingCollection();
                bindings.BindToggle(enabled, toggle);
                bindings.BindSlider(volume, slider);
                bindings.BindCommand(command, button);

                Assert.IsTrue(toggle.isOn);
                Assert.AreEqual(0.25f, slider.value);
                Assert.IsTrue(button.interactable);

                toggle.isOn = false;
                slider.value = 0.8f;
                button.onClick.Invoke();
                Assert.IsFalse(enabled.Value);
                Assert.AreEqual(0.8f, volume.Value);
                Assert.AreEqual(1, executionCount);

                canExecute = false;
                command.NotifyCanExecuteChanged();
                Assert.IsFalse(button.interactable);

                bindings.Dispose();
                toggle.isOn = true;
                slider.value = 0.1f;
                Assert.IsFalse(enabled.Value);
                Assert.AreEqual(0.8f, volume.Value);
            }
            finally
            {
                Object.DestroyImmediate(toggleObject);
                Object.DestroyImmediate(sliderObject);
                Object.DestroyImmediate(buttonObject);
            }
        }
    }
}
