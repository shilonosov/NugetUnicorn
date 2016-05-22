using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;

namespace NugetUnicorn.Business.SourcesParser
{
    public class NugetPackagesCollector
    {
        public static IEnumerable<ProjectInstance> Parse(string solutionFilePath)
        {
            var solutionFile = SolutionFile.Parse(solutionFilePath);
            return solutionFile.ProjectsInOrder
                               .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat || x.ProjectType == SolutionProjectType.WebProject)
                               .Select(x => new ProjectInstance(x.AbsolutePath));
        }
    }
}