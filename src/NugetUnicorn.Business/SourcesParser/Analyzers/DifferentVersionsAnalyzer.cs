using System;
using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

namespace NugetUnicorn.Business.SourcesParser
{
    public static class DifferentVersionsAnalyzer
    {
        public static IDictionary<ProjectPoco, IEnumerable<string>> ComposeDifferentVersionsReferencesErrors(
            IDictionary<ProjectPoco, IEnumerable<ReferenceInformation>> referenceMetadatas)
        {
            return referenceMetadatas.SelectMany(
                                         x => x.Value.Select(y => new Tuple<ProjectPoco, string, string>(x.Key, y.AssemblyName, y.Version)))
                                     .GroupBy(x => x.Item2)
                                     .Select(
                                         x => new Tuple<string, IList<IGrouping<string, Tuple<ProjectPoco, string, string>>>>(
                                             x.Key,
                                             x.GroupBy(y => y.Item3).ToList()))
                                     .Where(x => x.Item2.Count > 1)
                                     .Select(
                                         x =>
                                             new Tuple<string, IEnumerable<Tuple<string, IEnumerable<ProjectPoco>>>>(
                                                 x.Item1,
                                                 x.Item2.Select(
                                                     y => new Tuple<string, IEnumerable<ProjectPoco>>(y.Key, y.Select(z => z.Item1)))))
                                     .Select(
                                         x =>
                                             new Tuple<string, IEnumerable<Tuple<string, ProjectPoco>>>(
                                                 x.Item1,
                                                 x.Item2.SelectMany(y => y.Item2.Select(z => new Tuple<string, ProjectPoco>(y.Item1, z)))))
                                     .Select(
                                         x => new Tuple<string, IEnumerable<KeyValuePair<ProjectPoco, string>>>(
                                             x.Item1,
                                             ComposeProjectMessage(x.Item2, x.Item1)))
                                     .SelectMany(x => x.Item2)
                                     .GroupBy(x => x.Key)
                                     .ToDictionary(x => x.Key, x => x.Select(y => y.Value));
        }

        private static IEnumerable<KeyValuePair<ProjectPoco, string>> ComposeProjectMessage(
            IEnumerable<Tuple<string, ProjectPoco>> versionsAndProjects,
            string referenceName)
        {
            var enumerable = versionsAndProjects.ToArray();
            return
                enumerable.Select(
                    x =>
                        new KeyValuePair<ProjectPoco, string>(
                            x.Item2,
                            $"{x.Item2} reference {referenceName} {x.Item1} but there are projects which has same reference but with different version: {string.Join(", ", enumerable.Where(y => y.Item1 != x.Item1 && y.Item2 != x.Item2).Select(y => $"{y.Item2} -> {referenceName} v {y.Item1}"))}"
                        ));
        }
    }
}