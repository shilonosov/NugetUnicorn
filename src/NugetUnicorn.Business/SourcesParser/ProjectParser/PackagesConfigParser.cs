using System;
using System.Collections.Generic;
using System.Xml;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Models;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public interface IPackagesConfigParser
    {
        IList<PackageModel> ReadPackages(string configFilePath);
    }

    public class PackagesConfigParser : IPackagesConfigParser
    {
        public static IPackagesConfigParser Instance { get; } = new PackagesConfigParser();

        public IList<PackageModel> ReadPackages(string configFilePath)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentNullException(nameof(configFilePath), $"config file path is not set. something went wrong");
            }

            var result = new List<PackageModel>();

            var doc = new XmlDocument();
            doc.Load(configFilePath);

            var root = doc.DocumentElement;
            if (root == null)
            {
                throw new ApplicationException($"error reading packages configuration file {configFilePath} -- can't load!");
            }

            var nodes = root.SelectNodes("/packages");
            if (nodes == null)
            {
                throw new ApplicationException($"error reading packages configuration file {configFilePath} -- no root element found!");
            }

            for (var i = 0; i < nodes.Count; ++i)
            {
                var node = nodes.Item(i);
                if (node == null)
                {
                    continue;
                }

                var id = node.GetAttribute("id", string.Empty);
                var version = node.GetAttribute("version", string.Empty);
                var targetFramework = node.GetAttribute("culture", string.Empty);

                result.Add(new PackageModel(id, version, targetFramework));
            }

            return result;
        }
    }
}