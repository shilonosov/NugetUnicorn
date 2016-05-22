namespace NugetUnicorn.Business.FuzzyMatcher
{
    public abstract class SomeProbabilityMatchMetadata<T> : ProbabilityMatchMetadata<T>
    {
        protected SomeProbabilityMatchMetadata(T sample, ProbabilityMatch<T> match, double probability)
            : base(sample, probability)
        {
            Match = match;
        }

        public ProbabilityMatch<T> Match { get; }
    }
}