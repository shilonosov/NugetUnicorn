namespace NugetUnicorn.Business.FuzzyMatcher
{
    public class NoneProbabilityMatch<T> : ProbabilityMatch<T>
    {
        public override ProbabilityMatchMetadata<T> CalculateProbability(T dllMetadata)
        {
            return new NonePropabilityMatchMetadata<T>(dllMetadata);
        }
    }
}