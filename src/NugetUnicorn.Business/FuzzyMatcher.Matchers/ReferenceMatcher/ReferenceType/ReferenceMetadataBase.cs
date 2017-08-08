using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public abstract class ReferenceMetadataBase : SomeProbabilityMatchMetadata<ReferenceBase>
    {
        protected ReferenceMetadataBase(ReferenceBase sample, ProbabilityMatch<ReferenceBase> match, double probability)
            : base(sample, match, probability)
        {
        }
    }
}