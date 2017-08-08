using System.Collections.Generic;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Dto.Utils;

namespace NugetUnicorn.Dto
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