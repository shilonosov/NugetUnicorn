using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies
{
    public class OnNextStrategyReplace<T> : IOnNextStategy<T>
    {
        public void OnNext(IReactiveProperty<T> property, T t)
        {
            property.Value = t;
        }
    }
}