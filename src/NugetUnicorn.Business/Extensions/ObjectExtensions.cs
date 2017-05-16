using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.Extensions
{
    public interface ICanEvaluate<out TV>
    {
        TV Evaluate();

        IEnumerable<TV> EvaluateAll();
    }

    public interface ISwitch<out T, TV> : ICanEvaluate<TV>
    {
        ISwitch<T, TV> Case(Func<T, bool> caseFunc, Func<T, TV> action);

        ICanEvaluate<TV> Default(Func<T, TV> defaultAction);
    }

    public static class ObjectExtensions
    {
        public static ISwitch<T, TV> Switch<T, TV>(this T t)
        {
            return new Switch<T, TV>(t);
        }
    }

    public class Switch<T, TV> : ISwitch<T, TV>
    {
        private readonly T _subject;

        private readonly IList<Tuple<Func<T, bool>, Func<T, TV>>> _cases;

        public Switch(T subject)
        {
            _subject = subject;
            _cases = new List<Tuple<Func<T, bool>, Func<T, TV>>>();
        }

        public ISwitch<T, TV> Case(Func<T, bool> caseFunc, Func<T, TV> action)
        {
            _cases.Add(new Tuple<Func<T, bool>, Func<T, TV>>(caseFunc, action));
            return this;
        }

        public ICanEvaluate<TV> Default(Func<T, TV> defaultFunc)
        {
            _cases.Add(new Tuple<Func<T, bool>, Func<T, TV>>(x => true, defaultFunc));
            return this;
        }

        public TV Evaluate()
        {
            var result = _cases.FirstOrDefault(x => x.Item1(_subject));
            if (Equals(result, default(Tuple<Func<T, bool>, Func<T, TV>>)))
            {
                return default(TV);
            }
            return result.Item2(_subject);
        }

        public IEnumerable<TV> EvaluateAll()
        {
            return _cases.Where(x => x.Item1(_subject))
                         .Select(x => x.Item2(_subject));
        }
    }
}