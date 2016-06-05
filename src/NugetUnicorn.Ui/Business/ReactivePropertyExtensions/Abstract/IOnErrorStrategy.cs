using System;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract
{
    public interface IOnErrorStrategy<T>
    {
        void OnError(IReactiveProperty<T> property, Exception error);
    }
}