using System;
using System.Collections.Generic;

namespace NugetUnicorn.Utils.Extensions.EnumerableSwitch
{
    public interface IEnumerableSwitch<out T>
    {
        IEnumerableSwitch<T> Case(Func<T, bool> conditionFunc, Action<T> action);

        IEnumerable<T> Default(Action<T> action);
    }
}