using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business.FuzzyMatcher.Engine
{
    public class ProbabilityMatchEngine<T> : IProbabilityMatchEngine<T>
    {
        private readonly IList<ProbabilityMatch<T>> _matchList;

        public ProbabilityMatchEngine()
        {
            _matchList = new List<ProbabilityMatch<T>>();
        }

        public ProbabilityMatchMetadata<T> FindBestMatch(T sample)
        {
            //TODO: [DS] split CaltulateProbability so it would calculate probability only, compose metadate after sort
            return _matchList.Select(x => x.CalculateProbability(sample))
                             .OrderByDescending(x => x.Probability)
                             .FirstOrDefault()
                             .IfNull(() => new NonePropabilityMatchMetadata<T>(sample));
        }

        public ProbabilityMatchEngine<T> With(ProbabilityMatch<T> probabilityMatch)
        {
            _matchList.Add(probabilityMatch);
            return this;
        }
    }
}