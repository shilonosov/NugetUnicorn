using System;
using System.Linq;

using Microsoft.Build.Evaluation;

namespace NugetUnicorn.Business.Microsoft.Build
{
    public static class ProjectItemInstanceExtensions
    {
        private const string CONST_HINTPATH = "HintPath";

        public static string GetHintPath(this ProjectItem projectItem)
        {
            return projectItem?.Metadata
                              .FirstOrDefault(x => string.Equals(x.Name, CONST_HINTPATH))
                              ?
                              .EvaluatedValue;
        }

        public static string GetMetadataPrintStrign(this ProjectItem projectItem)
        {
            return string.Join(Environment.NewLine, projectItem.Metadata.Select(x => $"{x.Name}: {x}"));
        }
    }
}