using System;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract
{
    public abstract class ReactivePropertyObserverBridge<T> : IObserver<T>
    {
        private readonly IReactiveProperty<T> _reactiveProperty;

        private readonly IOnNextStategy<T> _onNextStategy;

        private readonly IOnErrorStrategy<T> _onErrorStrategy;

        private readonly IOnCompleteStrategy<T> _onCompleteStrategy;

        protected ReactivePropertyObserverBridge(IReactiveProperty<T> reactiveProperty,
                                                 IOnNextStategy<T> onNextStategy,
                                                 IOnErrorStrategy<T> onErrorStrategy,
                                                 IOnCompleteStrategy<T> onCompleteStrategy)
        {
            _reactiveProperty = reactiveProperty;
            _onNextStategy = onNextStategy;
            _onErrorStrategy = onErrorStrategy;
            _onCompleteStrategy = onCompleteStrategy;
        }

        public void OnNext(T value)
        {
            _onNextStategy.OnNext(_reactiveProperty, value);
        }

        public void OnError(Exception error)
        {
            _onErrorStrategy.OnError(_reactiveProperty, error);
        }

        public void OnCompleted()
        {
            _onCompleteStrategy.OnComplete(_reactiveProperty);
        }
    }
}