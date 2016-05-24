using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher;
using NugetUnicorn.Business.SourcesParser;

namespace NUgetUnicorn.Console
{
    public static class ProjectItemInstanceExtensions
    {
        private const string CONST_HINTPATH = "HintPath";

        public static string GetHintPath(this ProjectItemInstance projectItemInstance)
        {
            return projectItemInstance?.Metadata
                .FirstOrDefault(x => string.Equals(x.Name, CONST_HINTPATH))?
                .EvaluatedValue;
        }
    }

    public static class ProjectInstanceExtensions
    {
        private const string CONS_TARGET_FILE_NAME = "TargetFileName";

        private const string CONS_MSBUILD_PROJECT_NAME = "MSBuildProjectName";

        public static string GetTargetFileName(this ProjectInstance projectInstance)
        {
            return projectInstance.Properties?
                                  .FirstOrDefault(y => string.Equals(y.Name, CONS_TARGET_FILE_NAME))?
                                  .EvaluatedValue;
        }

        public static string GetProjectName(this ProjectInstance projectInstance)
        {
            return projectInstance?.Properties?
                                   .FirstOrDefault(y => string.Equals(y.Name, CONS_MSBUILD_PROJECT_NAME))?
                                   .EvaluatedValue;
        }
    }


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

    class Program
    {
        static void Main(string[] args)
        {
            //[DS] absolute path required
            var projects = NugetPackagesCollector.Parse(@"D:\dev\Projects\NugetUnicorn\src\NugetUnicorn.sln")
                .ToList();
            var matcher = new ProbabilityMatchEngine<ProjectItemInstance>();
            matcher.With(new ReferenceMatcher.NugetReference())
                   .With(new ReferenceMatcher.SystemReference())
                   .With(new ReferenceMatcher.ExplicitReference())
                   .With(new ReferenceMatcher.DllReference())
                   .With(new ReferenceMatcher.ProjectReference());

            var references = projects.Select(
                x =>
                    {
                        var name = x.GetProjectName() ?? "NO PROJECT NAME";
                        ConsoleEx.WriteLine(ConsoleColor.Green, $"PROJECT: {name}");
                        return x.Items;
                    })
                    //.Last()
                    .SelectMany(x => x)
                    .Select(x => matcher.FindBestMatch(x))
                    //.Switch()
                    //.Case(x =>
                    //    {
                    //        var y = x as SomeProbabilityMatchMetadata<ProjectItemInstance>;
                    //        return y != null && y.Probability > 0d;
                    //    }, x => ConsoleEx.WriteLine(ConsoleColor.Green, $"{x.GetType().Name} -- [{x.Sample.EvaluatedInclude}]"))
                    //.Default(x => ConsoleEx.WriteLine(ConsoleColor.Red, $"{x.GetType().Name} -- [{x.Sample.EvaluatedInclude}]"))
                    .Where(x => x is SomeProbabilityMatchMetadata<ProjectItemInstance>)
                    .Cast<SomeProbabilityMatchMetadata<ProjectItemInstance>>()
                    .Where(x => x.Probability > 0)
                    .Do(x => { System.Console.WriteLine($"{x.GetType().Name} -- {x.Sample.EvaluatedInclude}"); })
                    .Where(x => x is ReferenceMatcher.DllReference.DllMetadata)
                    .Cast<ReferenceMatcher.DllReference.DllMetadata>();

            System.Console.WriteLine();
            System.Console.WriteLine("Wrong references below:");
            System.Console.WriteLine();

            var wrongReferenceMatcher = new ProbabilityMatchEngine<ReferenceMatcher.DllReference.DllMetadata>();
            wrongReferenceMatcher.With(new WrongReferenceMatcher(projects));
            references.Select(x => wrongReferenceMatcher.FindBestMatch(x))
                .Where(x => x is WrongReferenceMatcher.WrongReferencePropabilityMetadata)
                .Cast<WrongReferenceMatcher.WrongReferencePropabilityMetadata>()
                .Where(x => x.Probability > 0)
                .Do(
                    x =>
                        {
                            ConsoleEx.WriteLine(ConsoleColor.Red, $"{x.Probability} -- {x.Project.GetProjectName()} to {x.Reference} ({x.SuspectedProject.GetProjectName()})");
                        });

            System.Console.ReadLine();
        }
    }
}