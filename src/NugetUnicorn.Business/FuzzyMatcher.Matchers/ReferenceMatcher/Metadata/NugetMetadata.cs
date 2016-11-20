using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata
{
    public class NugetMetadata : ExistingReferenceMetadataBase
    {
        public NugetMetadata(Reference sample, ProbabilityMatch<ReferenceBase> match, double probability)
            : base(sample, match, probability, sample.HintPath)
        {
        }
    }
}