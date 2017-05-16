using System.Collections.Generic;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;
using NugetUnicorn.Business.Utils;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public interface IProjectPoco
    {
        IReadOnlyCollection<ReferenceBase> References { get; }

        string TargetName { get; }

        string Name { get; }

        string AppConfigPath { get; }

        string PackagesConfigPath { get; }

        FilePath ProjectFilePath { get; }
    }
}