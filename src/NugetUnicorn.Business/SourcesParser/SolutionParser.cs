using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace NugetUnicorn.Business.SourcesParser
{
    public class SolutionParser
    {
        public static IEnumerable<Project> GetProjects(string solutionFilePath)
        {
            var solutionFile = SolutionFile.Parse(solutionFilePath);
            return solutionFile.ProjectsInOrder
                               .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat || x.ProjectType == SolutionProjectType.WebProject)
                               .Select(x => new Project(x.AbsolutePath));
        }
    }
}