using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.Microsoft.Build;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher
{
    public class ReferenceMatcher
    {
        public class ReferenceInformation
        {
            public string AssemblyName { get; private set; }
            public string Version { get; private set; }

            public ReferenceInformation(string fullPath)
            {
                var assembly = Assembly.LoadFile(fullPath);
                var assemblyName = assembly.GetName();
                AssemblyName = assemblyName.Name;
                Version = assemblyName.Version.ToString();
            }

            public ReferenceInformation(string assemblyName, string version)
            {
                AssemblyName = assemblyName;
                Version = version;
            }

            public override string ToString()
            {
                return $"{AssemblyName}, {Version}";
            }
        }

        public abstract class ReferenceMetadataBase : SomeProbabilityMatchMetadata<ProjectItem>
        {
            protected ReferenceMetadataBase(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                : base(sample, match, probability)
            {
            }
        }

        public abstract class ExistingReferenceMetadataBase : ReferenceMetadataBase
        {
            protected ExistingReferenceMetadataBase(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                : base(sample, match, probability)
            {
            }

            public abstract ReferenceInformation GetReferenceInformation();
        }

        private const string CONST_REFERENCE = "Reference";

        private const string CONST_PRIVATE = "Private";

        private const string CONST_HINTPATH = "HintPath";

        private const string CONST_TRUE = "True";

        public class ProjectReference : ProbabilityMatch<ProjectItem>
        {
            private const string CONST_PROJECT_REFERENCE = "ProjectReference";

            public class ProjectMetadata : ExistingReferenceMetadataBase
            {
                public ProjectMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                    : base(sample, match, probability)
                {
                }

                public override ReferenceInformation GetReferenceInformation()
                {
                    return new ReferenceInformation("i am no implement", "implement me");
                }
            }

            public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
            {
                if (string.Equals(dataSample.ItemType, CONST_PROJECT_REFERENCE))
                {
                    return new ProjectMetadata(dataSample, this, 1);
                }
                return base.CalculateProbability(dataSample);
            }
        }

        public class NugetReference : ProbabilityMatch<ProjectItem>
        {
            public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
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

            public class NugetMetadata : ExistingReferenceMetadataBase
            {
                public NugetMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                    : base(sample, match, probability)
                {
                }

                public override ReferenceInformation GetReferenceInformation()
                {
                    var directory = Sample.GetMetadataValue("RootDir") + Sample.GetMetadataValue("Directory");
                    var relativePath = Sample.GetHintPath();
                    var fullPath = Path.Combine(directory, relativePath);
                    return new ReferenceInformation(fullPath);
                }
            }
        }

        public class DllReference : ProbabilityMatch<ProjectItem>
        {
            public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
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

            public class DllMetadata : ExistingReferenceMetadataBase
            {
                public DllMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                    : base(sample, match, probability)
                {
                }

                public override ReferenceInformation GetReferenceInformation()
                {
                    var directory = Sample.GetMetadataValue("RootDir") + Sample.GetMetadataValue("Directory");
                    var relativePath = Sample.GetHintPath();
                    var fullPath = Path.Combine(directory, relativePath);
                    return new ReferenceInformation(fullPath);
                }
            }
        }

        public class SystemReference : ProbabilityMatch<ProjectItem>
        {
            //todo: check if this condition is valid
            public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
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

            public class SystemMetadata : ReferenceMetadataBase
            {
                public SystemMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }

        public class ExplicitReference : ProbabilityMatch<ProjectItem>
        {
            private const string CONST_EXPLICIT_REFERENCE = "_ExplicitReference";

            public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
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

            public class ExplicitMetadata : ReferenceMetadataBase
            {
                public ExplicitMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability)
                    : base(sample, match, probability)
                {
                }
            }
        }
    }
}