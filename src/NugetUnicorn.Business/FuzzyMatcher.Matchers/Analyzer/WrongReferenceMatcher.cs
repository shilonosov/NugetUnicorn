using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NugetUnicorn.Business.FuzzyMatcher.Engine;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Exceptions;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer
{
    public class WrongReferenceMatcher : ProbabilityMatch<DllMetadata>
    {
        private readonly IDictionary<string, IProjectPoco> _projectsCollection;

        public WrongReferenceMatcher(IEnumerable<IProjectPoco> projectsCollection)
        {
            var projectPocos = projectsCollection.ToArray();
            ValidateProjects(projectPocos);
            _projectsCollection = projectPocos.ToDictionary(x => x.TargetName, x => x);
        }

        private void ValidateProjects(IProjectPoco[] projectPocos)
        {
            var duplicateProjectOutputNames = projectPocos.GroupBy(x => x.TargetName)
                                                          .Where(x => x.Count() > 1)
                                                          .Select(x => $"Projects [{string.Join(", ", x.Select(y => y.Name))}] has the same output name: [{x.Key}]")
                                                          .ToArray();
            if (duplicateProjectOutputNames.Any())
            {
                var message = string.Join(Environment.NewLine, duplicateProjectOutputNames);
                throw new DuplicateProjectOutputNameException(message);
            }
        }

        public override ProbabilityMatchMetadata<DllMetadata> CalculateProbability(DllMetadata dataSample)
        {
            var sampleProjectPath = dataSample.SampleDetails.HintPath ?? string.Empty;
            var fileName = Path.GetFileName(sampleProjectPath);

            if (_projectsCollection.ContainsKey(fileName))
            {
                var suspectedProject = _projectsCollection[fileName];
                return new WrongReferencePropabilityMetadata(dataSample, this, 1d, sampleProjectPath, suspectedProject);
            }
            return base.CalculateProbability(dataSample);
        }
    }
}