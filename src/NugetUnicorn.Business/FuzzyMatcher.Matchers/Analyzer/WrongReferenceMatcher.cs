using System.Collections.Generic;
using System.IO;
using System.Linq;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer
{
    public class WrongReferenceMatcher : ProbabilityMatch<DllMetadata>
    {
        private readonly IDictionary<string, ProjectPoco> _projectsCollection;

        public WrongReferenceMatcher(IEnumerable<ProjectPoco> projectsCollection)
        {
            _projectsCollection = projectsCollection.ToDictionary(x => x.TargetName, x => x);
        }

        public override ProbabilityMatchMetadata<DllMetadata> CalculateProbability(DllMetadata dataSample)
        {
            var sampleProjectPath = dataSample.SampleDetails.HintPath ?? string.Empty;
            var fileName = Path.GetFileName(sampleProjectPath);

            if (_projectsCollection.ContainsKey(fileName))
            {
                var suspectedProject = _projectsCollection[fileName];
                return new WrongReferencePropabilityMetadata(dataSample, this, 1d, sampleProjectPath, suspectedProject);
            }
            return base.CalculateProbability(dataSample);
        }
    }
}