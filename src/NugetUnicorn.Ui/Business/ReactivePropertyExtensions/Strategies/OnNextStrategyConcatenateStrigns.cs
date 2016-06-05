using NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Abstract;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Business.ReactivePropertyExtensions.Strategies
{
    public class OnNextStrategyConcatenateStrigns : IOnNextStategy<string>
    {
        private readonly string _prefix;

        private readonly string _postfix;

        public OnNextStrategyConcatenateStrigns(string prefix, string postfix)
        {
            _prefix = prefix;
            _postfix = postfix;
        }

        public void OnNext(IReactiveProperty<string> property, string t)
        {
            property.Value += _prefix + t + _postfix;
        }
    }
}