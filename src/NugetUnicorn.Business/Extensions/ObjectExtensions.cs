using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NugetUnicorn.Business.Extensions.EnumerableSwitch;

namespace NugetUnicorn.Business.Extensions
{
    public interface ISwitch<T>
    {
        ISwitch<T> Case(Func<T, bool> caseFunc, Action<T> action);

        void Default(Action<T> defaultAction);

        void Evaluate(T t);
    }

    public static class ObjectExtensions
    {
        public static ISwitch<T> Switch<T>(this T t, bool isExclusive)
        {
            return new Switch<T>(isExclusive);
        }
    }

    public class Switch<T> : ISwitch<T>
    {
        private readonly bool _breakOnFirstMatch;

        private readonly IList<Tuple<Func<T, bool>, Action<T>>> _cases;

        public Switch(bool breakOnFirstMatch)
        {
            _breakOnFirstMatch = breakOnFirstMatch;
            _cases = new List<Tuple<Func<T, bool>, Action<T>>>();
        }

        public ISwitch<T> Case(Func<T, bool> caseFunc, Action<T> action)
        {
            _cases.Add(new Tuple<Func<T, bool>, Action<T>>(caseFunc, action));
            return this;
        }

        public void Default(Action<T> defaultAction)
        {
            _cases.Add(new Tuple<Func<T, bool>, Action<T>>(x => true, defaultAction));
        }

        public void Evaluate(T t)
        {
            foreach (var thisCase in _cases)
            {
                var condition = thisCase.Item1;
                if (!condition(t))
                {
                    continue;
                }

                thisCase.Item2(t);
                if (_breakOnFirstMatch)
                {
                    break;
                }
            }
        }
    }
}
