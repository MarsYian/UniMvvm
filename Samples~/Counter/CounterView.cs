using UnityEngine;
using UnityEngine.UI;
using UniMvvm.Binding;
using UniMvvm.Unity.UGui;
using UniMvvm.Samples.Integration;

namespace UniMvvm.Samples.Counter
{
    public sealed class CounterView : UIViewBase<CounterViewModel>
    {
        [SerializeField] private Text countText;
        [SerializeField] private Button incrementButton;

        public override void Bind()
        {
            var bindings = BeginBind();
            try
            {
                bindings.Bind(ViewModel.Count, value => countText.text = value.ToString());
                bindings.BindCommand(ViewModel.Increment, incrementButton);
            }
            catch
            {
                Unbind();
                throw;
            }
        }
    }
}
