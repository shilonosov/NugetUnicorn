using System;

using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;
using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Bridge
{
    public class ReactivePropertyObserverBridgeStringAdd : ReactivePropertyObserverBridge<string>
    {
        public ReactivePropertyObserverBridgeStringAdd(IReactiveProperty<string> reactiveProperty)
            : base(
                reactiveProperty,
                new OnNextStrategyConcatenateStrigns(string.Empty, Environment.NewLine),
                new OnErrorStrategyIgnore<string>(),
                new OnCompleteStrategyIgnore<string>())
        {
        }
    }
}