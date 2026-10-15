using UnityEngine;

namespace UniMvvm.Samples.Counter
{
    public sealed class CounterInstaller : MonoBehaviour
    {
        [SerializeField] private CounterView view;

        private void Awake()
        {
            view.SetViewModel(new CounterViewModel());
        }

        private void OnDestroy()
        {
            if (view != null)
                view.SetViewModel(null);
        }
    }
}
