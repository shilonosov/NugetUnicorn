using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract
{
    public interface IOnCompleteStrategy<T>
    {
        void OnComplete(IReactiveProperty<T> property);
    }
}