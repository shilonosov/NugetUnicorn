namespace NugetUnicorn.Business.FuzzyMatcher.Engine
{
    public abstract class SomeProbabilityMatchMetadata<T> : ProbabilityMatchMetadata<T>
    {
        public ProbabilityMatch<T> Match { get; }

        protected SomeProbabilityMatchMetadata(T sample, ProbabilityMatch<T> match, double probability)
            : base(sample, probability)
        {
            Match = match;
        }
    }
}