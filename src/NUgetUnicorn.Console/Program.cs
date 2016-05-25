using System;
using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.Microsoft.Build;
using NugetUnicorn.Business.SourcesParser;

namespace NUgetUnicorn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //[DS] absolute path required
            var projects = SolutionParser.GetProjects(@"D:\dev\Projects\NugetUnicorn\src\NugetUnicorn.sln")
                                         .ToList();
            var referenceMatcher = new ProbabilityMatchEngine<ProjectItemInstance>();
            referenceMatcher.With(new ReferenceMatcher.NugetReference())
                            .With(new ReferenceMatcher.SystemReference())
                            .With(new ReferenceMatcher.ExplicitReference())
                            .With(new ReferenceMatcher.DllReference())
                            .With(new ReferenceMatcher.ProjectReference());

            var wrongReferenceMatcher = new ProbabilityMatchEngine<ReferenceMatcher.DllReference.DllMetadata>();
            wrongReferenceMatcher.With(new WrongReferenceMatcher(projects));

            projects.Select(
                x =>
                    {
                        var name = x.GetProjectName() ?? "NO PROJECT NAME";
                        ConsoleEx.WriteLine(ConsoleColor.Green, $"PROJECT: {name}");
                        return x.Items;
                    })
                    .SelectMany(x => x)
                    .FindBestMatch(referenceMatcher)
                    .OfType<ReferenceMatcher.DllReference.DllMetadata>()
                    .Where(x => x.Probability > 0)
                    .FindBestMatch(wrongReferenceMatcher)
                    .OfType<WrongReferenceMatcher.WrongReferencePropabilityMetadata>()
                    .Where(x => x.Probability > 0)
                    .Do(
                        x => { ConsoleEx.WriteLine(ConsoleColor.Red, $"{x.Probability} -- {x.Project.GetProjectName()} to {x.Reference} ({x.SuspectedProject.GetProjectName()})"); });

            var nugetPackageFileParser = new ProbabilityMatchEngine<ProjectItemInstance>();
            nugetPackageFileParser.With(new NugetPackageFileMatcher());

            projects.SelectMany(x => x.Items)
                    .FindBestMatch(nugetPackageFileParser)
                    .OfType<NugetPackageFileMatcher.NugetPackageFilePropabilityMetadata>()
                    .Where(x => x.Probability > 0)
                    .Do(x => { System.Console.WriteLine($"{x.FullPath}"); });

            System.Console.ReadLine();
        }
    }

    public class NugetPackageFileMatcher : ProbabilityMatch<ProjectItemInstance>
    {
        public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
        {
            if (!string.Equals(dataSample.ItemType, "None"))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFilename = dataSample.MetadataNames.Any(x => string.Equals(x, "Filename"));
            if (!hasFilename)
            {
                return base.CalculateProbability(dataSample);
            }

            var filename = dataSample.GetMetadataValue("Identity");
            if (string.IsNullOrEmpty(filename) || !string.Equals(filename, "packages.config"))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFullPath = dataSample.MetadataNames.Any(x => string.Equals(x, "FullPath"));
            if (!hasFullPath)
            {
                return base.CalculateProbability(dataSample);
            }

            var fullPath = dataSample.GetMetadataValue("FullPath");
            if (string.IsNullOrEmpty(fullPath))
            {
                return base.CalculateProbability(dataSample);
            }

            return new NugetPackageFilePropabilityMetadata(dataSample, this, 1d, fullPath);
        }

        public class NugetPackageFilePropabilityMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
        {
            public string FullPath { get; }

            public NugetPackageFilePropabilityMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability, string fullPath)
                : base(sample, match, probability)
            {
                FullPath = fullPath;
            }
        }
    }
}