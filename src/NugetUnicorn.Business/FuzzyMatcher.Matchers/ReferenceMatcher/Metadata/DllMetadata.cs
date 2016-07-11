using System.IO;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata
{
    public class DllMetadata : ExistingReferenceMetadataBase
    {
        public Reference SampleDetails { get; private set; }

        public DllMetadata(Reference sample, ProbabilityMatch<ReferenceBase> match, double probability)
            : base(sample, match, probability, sample.HintPath)
        {
            SampleDetails = sample;
        }
    }
}