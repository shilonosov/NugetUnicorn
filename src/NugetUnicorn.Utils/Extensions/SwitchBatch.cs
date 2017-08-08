using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace NugetUnicorn.Utils.Extensions
{
    public class Context
    {
        private readonly IDictionary<int, object> _contextDictionary;

        public Context()
        {
            _contextDictionary = new Dictionary<int, object>();
        }

        public void Set<T>(int key, T value)
        {
            _contextDictionary[key] = value;
        }

        public bool Get<T>(int key, ref T value)
        {
            if (_contextDictionary.ContainsKey(key))
            {
                value = (T)_contextDictionary[key];
                return true;
            }
            return false;
        }
    }

    public interface ICaseSwitch<in T, TV>
    {
        bool Handle(T subject, ref TV value, Context context);
    }

    public class SwitchBatch<T, TV>
    {
        private readonly ICaseSwitch<T, TV>[] _cases;

        private readonly IScheduler _scheduler;

        public SwitchBatch(ICaseSwitch<T, TV>[] cases, IScheduler scheduler = null)
        {
            _cases = cases;
            _scheduler = scheduler ?? CurrentThreadScheduler.Instance;
        }

        public TV Evaluate(T subject)
        {
            var context = new Context();
            return _cases.ToObservable()
                .ObserveOn(_scheduler)
                .Select(x => EvaluateInternal(subject, x, context))
                .FirstOrDefaultAsync(x => x.Item1)
                .Wait()
                .Item2;
        }

        private static Tuple<bool, TV> EvaluateInternal(T subject, ICaseSwitch<T, TV> x, Context context)
        {
            var value = default(TV);
            var matched = x.Handle(subject, ref value, context);
            return new Tuple<bool, TV>(matched, value);
        }
    }
}