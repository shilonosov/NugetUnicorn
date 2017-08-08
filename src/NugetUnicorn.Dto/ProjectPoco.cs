using System.Collections.Generic;
using System.IO;
using System.Linq;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Dto.Utils;
using NugetUnicorn.Utils.Extensions;

namespace NugetUnicorn.Dto
{
    public class ProjectPoco : IProjectPoco
    {
        public TargetFramework TargetFramework { get; }

        public ProjectPoco(string fullPath, IEnumerable<ProjectStructureItem> projectStructure)
        {
            ProjectFilePath = new FilePath(fullPath);

            var references = new List<ReferenceBase>();

            var projectOutputName = string.Empty;
            var projectOutputType = string.Empty;
            var appConfigRelativePath = string.Empty;
            var packagesConfigRelativePath = string.Empty;
            TargetFramework targetFramework = null;
            var compilableItems = new List<string>();

            projectStructure.Switch()
                .Case(x => x is ReferenceBase, x => references.Add(x as ReferenceBase))
                .Case(x => x is AssemblyName, x => projectOutputName = (x as AssemblyName).Name)
                .Case(x => x is OutputType, x => projectOutputType = (x as OutputType).Extension)
                .Case(x => x is AppConfigItem, x => appConfigRelativePath = (x as AppConfigItem).RelativePath)
                .Case(x => x is TargetFramework, x => targetFramework = x as TargetFramework)
                .Case(x => x is PackagesConfigItem,
                    x => packagesConfigRelativePath = (x as PackagesConfigItem).RelativePath)
                .Case(x => x is CompilableItem, x => compilableItems.Add((x as CompilableItem).RelativePath))
                .Default(x => { })
                .Do(x => { });

            References = references.AsReadOnly();
            TargetName = $"{projectOutputName}.{projectOutputType}";
            Name = projectOutputName;
            AppConfigPath = string.IsNullOrEmpty(appConfigRelativePath)
                ? null
                : Path.Combine(ProjectFilePath.DirectoryPath, appConfigRelativePath);
            TargetFramework = targetFramework;
            PackagesConfigPath = string.IsNullOrEmpty(packagesConfigRelativePath)
                ? null
                : Path.Combine(ProjectFilePath.DirectoryPath, packagesConfigRelativePath);
            CompilableItems = compilableItems.Where(x => !string.IsNullOrEmpty(x))
                .Select(x => Path.Combine(ProjectFilePath.DirectoryPath, x))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyCollection<ReferenceBase> References { get; }

        public string TargetName { get; }

        public string Name { get; }

        public string AppConfigPath { get; }

        public string PackagesConfigPath { get; }

        public FilePath ProjectFilePath { get; }

        public IReadOnlyCollection<string> CompilableItems { get; }

        public override string ToString()
        {
            return $"{Name} - {TargetName}";
        }
    }
}