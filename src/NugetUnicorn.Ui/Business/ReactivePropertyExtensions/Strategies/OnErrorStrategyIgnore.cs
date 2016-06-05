using System;

using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies
{
    public class OnErrorStrategyIgnore<T> : IOnErrorStrategy<T>
    {
        public void OnError(IReactiveProperty<T> property, Exception error)
        {
        }
    }
}