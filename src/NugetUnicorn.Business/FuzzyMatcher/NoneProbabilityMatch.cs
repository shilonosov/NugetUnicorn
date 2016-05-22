namespace NugetUnicorn.Business.FuzzyMatcher
{
    public class NoneProbabilityMatch<T> : ProbabilityMatch<T>
    {
        public override ProbabilityMatchMetadata<T> CalculateProbability(T dataSample)
        {
            return new NonePropabilityMatchMetadata<T>(dataSample);
        }
    }
}