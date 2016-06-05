using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;
using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Bridge
{
    public class ReactivePropertyObserverBridgeStringReplace : ReactivePropertyObserverBridge<string>
    {
        public ReactivePropertyObserverBridgeStringReplace(IReactiveProperty<string> reactiveProperty)
            : base(
                reactiveProperty,
                new OnNextStrategyReplace<string>(),
                new OnErrorStrategyIgnore<string>(),
                new OnCompleteStrategyIgnore<string>())
        {
        }
    }
}