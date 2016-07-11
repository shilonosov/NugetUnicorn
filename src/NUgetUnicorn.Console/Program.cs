using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.SolutionFileParsers;
using NugetUnicorn.Business.Microsoft.Build;
using NugetUnicorn.Business.SourcesParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

using ProjectReference = NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType.ProjectReference;

namespace NUgetUnicorn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //[DS] absolute path required
            var projects = SolutionParser.GetProjects(@"D:\dev\Projects\NugetUnicorn\src\NugetUnicorn.sln")
                                         .ToList();


            var referenceMatcher = new ProbabilityMatchEngine<ReferenceBase>();
            referenceMatcher.With(new NugetReference())
                            .With(new SystemReference())
                            .With(new DllReference())
                            .With(new ProjectReference());

            var wrongReferenceMatcher = new ProbabilityMatchEngine<DllMetadata>();
            wrongReferenceMatcher.With(new WrongReferenceMatcher(projects));

            projects.Select(
                x =>
                    {
                        var name = x.Name ?? "NO PROJECT NAME";
                        ConsoleEx.WriteLine(ConsoleColor.Green, $"PROJECT: {name}");
                        return x;
                    })
                    .SelectMany(x => x.References)
                    .FindBestMatch<ReferenceBase, DllMetadata>(referenceMatcher, 0d)
                    .FindBestMatch<DllMetadata, WrongReferencePropabilityMetadata>(wrongReferenceMatcher, 0d)
                    .Do(
                        x => { ConsoleEx.WriteLine(ConsoleColor.Red, $"{x.Probability} -- to {x.Reference} ({x.SuspectedProject.Name})"); });

            //var nugetPackageFileParser = new ProbabilityMatchEngine<ReferenceBase>();
            //nugetPackageFileParser.With(new NugetPackageFileMatcher());

            //projects.SelectMany(x => x.References)
            //        .FindBestMatch<ReferenceBase, NugetPackageFileMatcher.NugetPackageFilePropabilityMetadata>(nugetPackageFileParser, 0d)
            //        .Do(x => { System.Console.WriteLine($"{x.FullPath}"); });

            System.Console.ReadLine();
        }
    }
}