using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Dto;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.Extensions;

namespace NugetUnicorn.Business.SourcesParser.Analyzers
{
    public static class IncorrectRedirectsAndReferencesAnalyzer
    {
        public static IDictionary<ProjectPoco, IEnumerable<string>> ComposeRedirectsVsReferencesErrors(
            IList<ProjectPoco> projects)
        {
            var appConfigParser = AppConfigParser.Instance;
            return projects.Where(x => x.AppConfigPath != null)
                           .ToDictionary(
                               x => x,
                               x =>
                                   {
                                       var bindings = appConfigParser.ReadBindings(x.AppConfigPath);
                                       var projectReferences = x.References.Select(y => y as ProjectReference)
                                                                .Where(y => y != null)
                                                                .Select(y => y.Name);
                                       var references = x.References.Select(y => y as Reference)
                                                         .Where(y => y != null)
                                                         .ToArray();
                                       var dllReferences = references.Select(y => Path.GetFileNameWithoutExtension(y.HintPath))
                                                                     .Concat(references.Select(y => Path.GetFileNameWithoutExtension(y.HintPath)))
                                                                     .Where(y => y != null);
                                       var referenceNames = projectReferences.Concat(dllReferences);

                                       return bindings.Where(y => referenceNames.FirstOrDefault(z => String.Equals(z, y.Name)) == null)
                                                      .Select(
                                                          y =>
                                                              $"config file has an assembly binding redirect to [{y}] but project doesn't reference lib with the same name");
                                   })
                           .Where(x => x.Value.Any())
                           .ToDictionary();
        }
    }
}