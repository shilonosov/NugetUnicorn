namespace NugetUnicorn.Utils.FuzzyMatcher.Engine
{
    public class NonePropabilityMatchMetadata<T> : ProbabilityMatchMetadata<T>
    {
        public NonePropabilityMatchMetadata(T sample)
            : base(sample, 0d)
        {
        }
    }
}