using System;
using System.Collections.Generic;
using System.Linq;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Dto;

namespace NugetUnicorn.Business.SourcesParser.Analyzers
{
    public static class IncorrectBindingsAnalyzer
    {
        public static IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<string>>> ComposeBindingsErrors(
            IList<ProjectPoco> projects,
            IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referencesByProjects)
        {
            var appConfigParser = AppConfigParser.Instance;
            var projectBindings = projects.Where(x => x.AppConfigPath != null)
                                          .ToDictionary(
                                              x => x,
                                              x => appConfigParser.ReadBindings(x.AppConfigPath));

            return projectBindings.Join(
                referencesByProjects,
                x => x.Key,
                y => y.Key,
                (x, y) =>
                    {
                        var bindingReferences = x.Value.Select(z => new ReferenceInformation(z.Name, z.NewVersion));

                        var incorrect = bindingReferences.Join(
                                                             y.Value,
                                                             x1 => x1.AssemblyName,
                                                             y1 => y1.AssemblyName,
                                                             (x1, y1) => new Tuple<ReferenceInformation, ReferenceInformation>(x1, y1))
                                                         .Where(z => !String.Equals(z.Item1.Version, z.Item2.Version))
                                                         .Select(
                                                             z =>
                                                                 $"reference mismatch: redirect: {z.Item1.ToString()}, reference: {z.Item2.ToString()}");

                        return new KeyValuePair<ProjectPoco, IEnumerable<string>>(x.Key, incorrect);
                    });
        }
    }
}