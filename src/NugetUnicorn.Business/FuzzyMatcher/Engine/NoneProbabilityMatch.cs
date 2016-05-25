namespace NugetUnicorn.Business.FuzzyMatcher.Engine
{
    public class NoneProbabilityMatch<T> : ProbabilityMatch<T>
    {
        public override ProbabilityMatchMetadata<T> CalculateProbability(T dllMetadata)
        {
            return new NonePropabilityMatchMetadata<T>(dllMetadata);
        }
    }
}