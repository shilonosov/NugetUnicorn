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
            try
            {
                return ProjectFileParser.Parse(x.AbsolutePath);
            }
            catch (Exception e)
            {
                throw new ApplicationException($"error parsing project [{x.ProjectName}] -- [{x.AbsolutePath}]", e);
            }
        }

        private static Project ComposeProject(ProjectInSolution x)
        {
            return new Project(x.AbsolutePath, null, null, ProjectCollection.GlobalProjectCollection, ProjectLoadSettings.IgnoreMissingImports);
        }
    }
}