using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType
{
    public class ProjectReference : ProbabilityMatch<ReferenceBase>
    {
        public override ProbabilityMatchMetadata<ReferenceBase> CalculateProbability(ReferenceBase dataSample)
        {
            var projectReference = dataSample as NugetUnicorn.Dto.Structure.ProjectReference;
            if (projectReference != null)
            {
                return new ProjectMetadata(projectReference, this, 1);
            }

            return base.CalculateProbability(dataSample);
        }
    }
}