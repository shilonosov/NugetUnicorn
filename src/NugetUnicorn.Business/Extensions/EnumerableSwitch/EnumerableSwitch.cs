using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.Extensions.EnumerableSwitch
{
    public class EnumerableSwitch<T> : IEnumerableSwitch<T>
    {
        private readonly IEnumerable<T> _enumerable;

        private readonly IList<EnumerableSwitchCase<T>> _caseList;

        public EnumerableSwitch(IEnumerable<T> enumerable)
        {
            _enumerable = enumerable;
            _caseList = new List<EnumerableSwitchCase<T>>();
        }

        public IEnumerableSwitch<T> Case(Func<T, bool> conditionFunc, Action<T> action)
        {
            _caseList.Add(new EnumerableSwitchCase<T>(conditionFunc, action));
            return this;
        }

        public IEnumerable<T> Default(Action<T> action)
        {
            return _enumerable.Select(
                x =>
                    {
                        var match = _caseList.FirstOrDefault(y => y.ConditionFunc(x));
                        if (match != null)
                        {
                            match.Action(x);
                        }
                        else
                        {
                            action(x);
                        }
                        return x;
                    });
        }
    }
}