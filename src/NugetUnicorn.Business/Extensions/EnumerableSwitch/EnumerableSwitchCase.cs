using System;

namespace NugetUnicorn.Business.Extensions.EnumerableSwitch
{
    public class EnumerableSwitchCase<T>
    {
        public Func<T, bool> ConditionFunc { get; }

        public Action<T> Action { get; }

        public EnumerableSwitchCase(Func<T, bool> conditionFunc, Action<T> action)
        {
            ConditionFunc = conditionFunc;
            Action = action;
        }
    }
}