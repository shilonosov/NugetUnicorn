using System.Collections.Generic;
using System.IO;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;
using NugetUnicorn.Business.Utils;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public class ProjectPoco
    {
        public IReadOnlyCollection<ReferenceBase> References { get; private set; }

        public string TargetName { get; }

        public string Name { get; }

        public string AppConfigPath { get; private set; }

        public string PackagesConfigPath { get; private set; }

        public FilePath ProjectFilePath { get; }

        public ProjectPoco(string fullPath, IEnumerable<ProjectStructureItem> projectStructure)
        {
            ProjectFilePath = new FilePath(fullPath);

            var references = new List<ReferenceBase>();

            var projectOutputName = string.Empty;
            var projectOutputType = string.Empty;
            var appConfigRelativePath = string.Empty;
            var packagesConfigRelativePath = string.Empty;

            projectStructure.Switch()
                            .Case(x => x is ReferenceBase, x => references.Add(x as ReferenceBase))
                            .Case(x => x is AssemblyName, x => projectOutputName = (x as AssemblyName).Name)
                            .Case(x => x is OutputType, x => projectOutputType = (x as OutputType).Extension)
                            .Case(x => x is AppConfigItem, x => appConfigRelativePath = (x as AppConfigItem).RelativePath)
                            .Case(x => x is PackagesConfigItem, x => packagesConfigRelativePath = (x as PackagesConfigItem).RelativePath)
                            .Default(x => { })
                            .Do(x => { });

            References = references.AsReadOnly();
            TargetName = $"{projectOutputName}.{projectOutputType}";
            Name = projectOutputName;
            AppConfigPath = string.IsNullOrEmpty(appConfigRelativePath) ? null : Path.Combine(ProjectFilePath.DirectoryPath, appConfigRelativePath);
            PackagesConfigPath = string.IsNullOrEmpty(packagesConfigRelativePath) ? null : Path.Combine(ProjectFilePath.DirectoryPath, packagesConfigRelativePath);
        }

        public override string ToString()
        {
            return $"{Name} - {TargetName}";
        }
    }
}