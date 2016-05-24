using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.FuzzyMatcher;

namespace NUgetUnicorn.Console
{
    public class ReferenceMatcher
    {
        private const string CONST_REFERENCE = "Reference";

        private const string CONST_PRIVATE = "Private";

        private const string CONST_HINTPATH = "HintPath";

        private const string CONST_TRUE = "True";

        public class ProjectReference : ProbabilityMatch<ProjectItemInstance>
        {
            private const string CONST_PROJECT_REFERENCE = "ProjectReference";

            public class ProjectMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public ProjectMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }

            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (string.Equals(dataSample.ItemType, CONST_PROJECT_REFERENCE))
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
                if (!string.Equals(dataSample.ItemType, CONST_REFERENCE))
                {
                    return base.CalculateProbability(dataSample);
                }

                var isPrivate = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_PRIVATE) && string.Equals(x.EvaluatedValue, CONST_TRUE));
                var hasHintPath = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_HINTPATH));
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

        public class DllReference : ProbabilityMatch<ProjectItemInstance>
        {
            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, CONST_REFERENCE))
                {
                    return base.CalculateProbability(dataSample);
                }

                var isPrivate = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_PRIVATE) && string.Equals(x.EvaluatedValue, CONST_TRUE));
                var hasHintPath = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_HINTPATH));
                if (!isPrivate && hasHintPath)
                {
                    return new DllMetadata(dataSample, this, 1d);
                }
                return base.CalculateProbability(dataSample);
            }

            public class DllMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
            {
                public DllMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }

        public class SystemReference : ProbabilityMatch<ProjectItemInstance>
        {
            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, CONST_REFERENCE))
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
            private const string CONST_EXPLICIT_REFERENCE = "_ExplicitReference";

            public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
            {
                if (!string.Equals(dataSample.ItemType, CONST_EXPLICIT_REFERENCE))
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