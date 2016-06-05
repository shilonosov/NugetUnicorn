using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract
{
    public interface IOnNextStategy<T>
    {
        void OnNext(IReactiveProperty<T> property, T t);
    }
}