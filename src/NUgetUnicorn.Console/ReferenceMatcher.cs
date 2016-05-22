using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.FuzzyMatcher;

namespace NUgetUnicorn.Console
{
    public class ReferenceMatcher
    {
        public class ProjectReference : ProbabilityMatch<ProjectItemInstance>
        {
            public class ProjectMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public ProjectMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }

            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (string.Equals(dataSample.ItemType, "ProjectReference"))
                {
                    return new ProjectMetadata(dataSample, this, 1);
                }
                return base.CalculateProbability(dataSample);
            }
        }

        public class NugetReference : ProbabilityMatch<ProjectItemInstance>
        {
            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, "Reference"))
                {
                    return base.CalculateProbability(dataSample);
                }

                var isPrivate = dataSample.Metadata.Any(x => string.Equals(x.Name, "Private") && string.Equals(x.EvaluatedValue, "True"));
                var hasHintPath = dataSample.Metadata.Any(x => string.Equals(x.Name, "HintPath"));
                if (isPrivate && hasHintPath)
                {
                    return new NugetMetadata(dataSample, this, 1d);
                }
                return base.CalculateProbability(dataSample);
            }

            public class NugetMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public NugetMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }

        public class SystemReference : ProbabilityMatch<ProjectItemInstance>
        {
            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, "Reference"))
                {
                    return base.CalculateProbability(dataSample);
                }

                var condition = !dataSample.Metadata.Any();
                if (condition)
                {
                    return new SystemMetadata(dataSample, this, 1d);
                }
                return base.CalculateProbability(dataSample);
            }

            public class SystemMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public SystemMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }

        public class ExplicitReference : ProbabilityMatch<ProjectItemInstance>
        {
            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, "_ExplicitReference"))
                {
                    return base.CalculateProbability(dataSample);
                }

                var condition = !dataSample.Metadata.Any();
                if (condition)
                {
                    return new ExplicitMetadata(dataSample, this, 1d);
                }
                return base.CalculateProbability(dataSample);
            }

            public class ExplicitMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public ExplicitMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }
    }
}