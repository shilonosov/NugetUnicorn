using System;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public class DllReference : ProbabilityMatch<ReferenceBase>
    {
        public override ProbabilityMatchMetadata<ReferenceBase> CalculateProbability(ReferenceBase dataSample)
        {
            var reference = dataSample as Reference;

            if (reference == null)
            {
                return base.CalculateProbability(dataSample);
            }

            if (!reference.IsPrivate && !String.IsNullOrEmpty(reference.HintPath))
            {
                return new DllMetadata(reference, this, 1d);
            }
            return base.CalculateProbability(dataSample);
        }
    }
}