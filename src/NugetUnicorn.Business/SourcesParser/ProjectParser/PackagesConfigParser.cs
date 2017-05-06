using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Models;

using NuGet.Packaging;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public interface IPackagesConfigParser
    {
        IEnumerable<PackageReference> ReadPackages(string configFilePath);
    }

    public class PackagesConfigParser : IPackagesConfigParser
    {
        public static IPackagesConfigParser Instance { get; } = new PackagesConfigParser();

        public IEnumerable<PackageReference> ReadPackages(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentNullException(nameof(configFilePath), $"config file path is not set. something went wrong");
            }

            using (var fileStream = File.Open(configFilePath, FileMode.Open, FileAccess.Read))
            {
                var packageConfigReader = new PackagesConfigReader(fileStream);
                return packageConfigReader.GetPackages();
            }
        }
    }
}