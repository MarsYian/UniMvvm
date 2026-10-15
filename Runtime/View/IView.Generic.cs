using UniMvvm.Core;
namespace UniMvvm.View
{
    public interface IView<TViewModel> : IView where TViewModel : IViewModel
    {
        TViewModel ViewModel { get; }
    }
}