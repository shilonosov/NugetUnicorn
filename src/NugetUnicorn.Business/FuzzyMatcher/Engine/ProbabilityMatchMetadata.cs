namespace NugetUnicorn.Business.FuzzyMatcher.Engine
{
    public abstract class ProbabilityMatchMetadata<T>
    {
        public T Sample { get; }

        public double Probability { get; set; }

        protected ProbabilityMatchMetadata(T sample, double probability)
        {
            Sample = sample;
            Probability = probability;
        }
    }
}