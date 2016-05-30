using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.Microsoft.Build;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher
{
    public class WrongReferenceMatcher : ProbabilityMatch<ReferenceMatcher.DllReference.DllMetadata>
    {
        private readonly IDictionary<string, ProjectInstance> _projectsCollection;

        public WrongReferenceMatcher(IEnumerable<ProjectInstance> projectsCollection)
        {
            _projectsCollection = projectsCollection.ToDictionary(x => x.GetTargetFileName(), x => x);
        }

        public override ProbabilityMatchMetadata<ReferenceMatcher.DllReference.DllMetadata> CalculateProbability(ReferenceMatcher.DllReference.DllMetadata dataSample)
        {
            var sampleProjectPath = dataSample.Sample.GetHintPath() ?? string.Empty;
            var fileName = Path.GetFileName(sampleProjectPath);

            if (_projectsCollection.ContainsKey(fileName))
            {
                var suspectedProject = _projectsCollection[fileName];
                return new WrongReferencePropabilityMetadata(dataSample, this, 1d, sampleProjectPath, suspectedProject);
            }
            return base.CalculateProbability(dataSample);
        }

        public class WrongReferencePropabilityMetadata : SomeProbabilityMatchMetadata<ReferenceMatcher.DllReference.DllMetadata>
        {
            public ProjectInstance Project { get; }
            public ProjectInstance SuspectedProject { get; }
            public string Reference { get; }

            public WrongReferencePropabilityMetadata(ReferenceMatcher.DllReference.DllMetadata sample, ProbabilityMatch<ReferenceMatcher.DllReference.DllMetadata> match, double probability, string sampleProjectPath, ProjectInstance suspectedProject)
                : base(sample, match, probability)
            {
                Project = sample.Sample.Project;
                SuspectedProject = suspectedProject;
                Reference = sampleProjectPath;
            }
        }
    }
}