using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public class NugetReference : ProbabilityMatch<ReferenceBase>
    {
        public override ProbabilityMatchMetadata<ReferenceBase> CalculateProbability(ReferenceBase dataSample)
        {
            var reference = dataSample as Reference;
            if (reference == null)
            {
                return base.CalculateProbability(dataSample);
            }

            if (reference.IsPrivate && !string.IsNullOrEmpty(reference.HintPath))
            {
                return new NugetMetadata(reference, this, 1d);
            }
            return base.CalculateProbability(dataSample);
        }
    }
}