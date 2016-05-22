namespace NugetUnicorn.Business.FuzzyMatcher
{
    public abstract class ProbabilityMatch<T>
    {
        public virtual ProbabilityMatchMetadata<T> CalculateProbability(T dataSample)
        {
            return new NonePropabilityMatchMetadata<T>(dataSample);
        }
    }
}