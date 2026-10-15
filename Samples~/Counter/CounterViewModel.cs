using UniMvvm.Core;

namespace UniMvvm.Samples.Counter
{
    public sealed class CounterViewModel : IViewModel
    {
        public ObservableProperty<int> Count { get; } = new();
        public ICommand Increment { get; }

        public CounterViewModel()
        {
            Increment = new RelayCommand(() => Count.Value++);
        }
    }
}
