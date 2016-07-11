using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public class ProjectReference : ProbabilityMatch<ReferenceBase>
    {
        public override ProbabilityMatchMetadata<ReferenceBase> CalculateProbability(ReferenceBase dataSample)
        {
            var projectReference = dataSample as SourcesParser.ProjectParser.Structure.ProjectReference;
            if (projectReference != null)
            {
                return new ProjectMetadata(projectReference, this, 1);
            }

            return base.CalculateProbability(dataSample);
        }
    }
}