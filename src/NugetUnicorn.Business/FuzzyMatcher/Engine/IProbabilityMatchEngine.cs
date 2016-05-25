namespace NugetUnicorn.Business.FuzzyMatcher.Engine
{
    public interface IProbabilityMatchEngine<T>
    {
        ProbabilityMatchMetadata<T> FindBestMatch(T sample);
    }
}