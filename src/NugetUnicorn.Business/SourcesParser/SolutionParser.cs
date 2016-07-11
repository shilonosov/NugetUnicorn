using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.SourcesParser
{
    public class SolutionParser
    {
        private static readonly ProjectFileParser ProjectFileParser;

        static SolutionParser()
        {
            ProjectFileParser = new ProjectFileParser();
        }

        public static IEnumerable<ProjectPoco> GetProjects(string solutionFilePath)
        //public static IEnumerable<Project> GetProjects(string solutionFilePath)
        {
            var solutionFile = SolutionFile.Parse(solutionFilePath);
            return solutionFile.ProjectsInOrder
                               .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat || x.ProjectType == SolutionProjectType.WebProject)
                               .Select(ComposeProjectPoco);
        }

        private static ProjectPoco ComposeProjectPoco(ProjectInSolution x)
        {
            return ProjectFileParser.Parse(x.AbsolutePath);
        }

        private static Project ComposeProject(ProjectInSolution x)
        {
            return new Project(x.AbsolutePath, null, null, ProjectCollection.GlobalProjectCollection, ProjectLoadSettings.IgnoreMissingImports);
        }
    }
}