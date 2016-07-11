using System.IO;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata
{
    public class ProjectMetadata : ExistingReferenceMetadataBase
    {
        public ProjectMetadata(ReferenceBase sample, ProbabilityMatch<ReferenceBase> match, double probability)
            : base(sample, match, probability, sample.Include)
        {
        }

        public override ReferenceInformation GetReferenceInformation(ProjectPoco projectPoco)
        {
            return new ReferenceInformation("I am no implement. make me.", "make me");
        }
    }
}