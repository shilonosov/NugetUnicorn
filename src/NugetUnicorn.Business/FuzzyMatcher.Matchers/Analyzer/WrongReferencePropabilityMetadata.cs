using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer
{
    public class WrongReferencePropabilityMetadata : SomeProbabilityMatchMetadata<DllMetadata>
    {
        //public Project Project { get; }
        public IProjectPoco SuspectedProject { get; }

        public string Reference { get; }

        public WrongReferencePropabilityMetadata(DllMetadata sample,
                                                 ProbabilityMatch<DllMetadata> match,
                                                 double probability,
                                                 string sampleProjectPath,
                                                 IProjectPoco suspectedProject)
            : base(sample, match, probability)
        {
            //Project = sample.Sample.Project;
            SuspectedProject = suspectedProject;
            Reference = sampleProjectPath;
        }
    }
}