using System;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public class SystemReference : ProbabilityMatch<ReferenceBase>
    {
        //todo: check if this condition is valid
        public override ProbabilityMatchMetadata<ReferenceBase> CalculateProbability(ReferenceBase dataSample)
        {
            var reference = dataSample as Reference;
            if (reference == null)
            {
                return base.CalculateProbability(dataSample);
            }

            if (!reference.IsPrivate && String.IsNullOrEmpty(reference.HintPath) && !String.IsNullOrEmpty(reference.Include))
            {
                return new SystemMetadata(reference, this, 1d);
            }

            return base.CalculateProbability(dataSample);
        }

        public class SystemMetadata : ReferenceMetadataBase
        {
            public SystemMetadata(Reference sample, ProbabilityMatch<ReferenceBase> match, double probability)
                : base(sample, match, probability)
            {
            }
        }
    }
}