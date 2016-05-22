using System;
using System.Linq;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher;
using NugetUnicorn.Business.SourcesParser;

namespace NUgetUnicorn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //[DS] absolute path required
            var projects = NugetPackagesCollector.Parse(@"D:\dev\Projects\NugetUnicorn\src\NugetUnicorn.sln");
            var matcher = new ProbabilityMatchEngine<ProjectItemInstance>();
            matcher.With(new ReferenceMatcher.NugetReference())
                   .With(new ReferenceMatcher.SystemReference())
                   .With(new ReferenceMatcher.ExplicitReference())
                   .With(new ReferenceMatcher.ProjectReference());

            projects.Select(
                x =>
                    {
                        var name = x.Properties.FirstOrDefault(y => string.Equals(y.Name, "MSBuildProjectName"))?.EvaluatedValue ?? "NO PROJECT NAME";
                        ConsoleEx.WriteLine(ConsoleColor.Green, $"PROJECT: {name}");
                        return x.Items;
                    })
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
                    .ForEachItem(
                        x => { System.Console.WriteLine($"{x.GetType().Name} -- {x.Sample.EvaluatedInclude}"); });

            System.Console.ReadLine();
        }
    }
}