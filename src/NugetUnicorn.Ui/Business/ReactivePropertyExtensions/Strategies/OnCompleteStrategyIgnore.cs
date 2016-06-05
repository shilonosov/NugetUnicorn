using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies
{
    public class OnCompleteStrategyIgnore<T> : IOnCompleteStrategy<T>
    {
        public void OnComplete(IReactiveProperty<T> property)
        {
        }
    }
}