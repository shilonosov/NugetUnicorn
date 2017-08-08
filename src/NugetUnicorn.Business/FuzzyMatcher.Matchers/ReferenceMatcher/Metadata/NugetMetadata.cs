using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

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