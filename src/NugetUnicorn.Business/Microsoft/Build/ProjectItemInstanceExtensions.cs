using System;
using System.Linq;

using Microsoft.Build.Execution;

namespace NugetUnicorn.Business.Microsoft.Build
{
    public static class ProjectItemInstanceExtensions
    {
        private const string CONST_HINTPATH = "HintPath";

        public static string GetHintPath(this ProjectItemInstance projectItemInstance)
        {
            return projectItemInstance?.Metadata
                                       .FirstOrDefault(x => string.Equals(x.Name, CONST_HINTPATH))?
                                       .EvaluatedValue;
        }

        public static string GetMetadataPrintStrign(this ProjectItemInstance projectItemInstance)
        {
            return string.Join(Environment.NewLine, projectItemInstance.MetadataNames.Select(x => $"{x}: {projectItemInstance.GetMetadataValue(x)}"));
        }
    }
}