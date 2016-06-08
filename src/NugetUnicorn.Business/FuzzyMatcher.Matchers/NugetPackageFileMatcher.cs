using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Microsoft.Build.Execution;

using NugetUnicorn.Business.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers
{
    public class AppConfigFileReferenceMatcher : ProbabilityMatch<ProjectItemInstance>
    {
        private const string CONST_ITEM_TYPE_NONE = "None";

        private const string CONST_METADATA_NAME_FILENAME = "Filename";

        private const string CONST_METADATA_NAME_IDENTITY = "Identity";

        private const string CONST_FILENAME_PACKAGES_CONFIG = "app.config";

        private const string CONST_METADATA_NAME_FULLPATH = "FullPath";

        public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
        {
            if (!string.Equals(dataSample.ItemType, CONST_ITEM_TYPE_NONE))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFilename = dataSample.MetadataNames.Any(x => string.Equals(x, CONST_METADATA_NAME_FILENAME));
            if (!hasFilename)
            {
                return base.CalculateProbability(dataSample);
            }

            var filename = dataSample.GetMetadataValue(CONST_METADATA_NAME_IDENTITY);
            if (string.IsNullOrEmpty(filename) || !string.Equals(filename, CONST_FILENAME_PACKAGES_CONFIG, StringComparison.InvariantCultureIgnoreCase))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFullPath = dataSample.MetadataNames.Any(x => string.Equals(x, CONST_METADATA_NAME_FULLPATH));
            if (!hasFullPath)
            {
                return base.CalculateProbability(dataSample);
            }

            var fullPath = dataSample.GetMetadataValue(CONST_METADATA_NAME_FULLPATH);
            if (string.IsNullOrEmpty(fullPath))
            {
                return base.CalculateProbability(dataSample);
            }

            return new AppConfigFilePropabilityMetadata(dataSample, this, 1d, fullPath);
        }

        public class AppConfigFilePropabilityMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
        {
            public class BindingRedirectModel
            {
                public string Identity { get; private set; }
                public string NewVersion { get; private set; }

                public BindingRedirectModel(string identity, string newVersion)
                {
                    Identity = identity;
                    NewVersion = newVersion;
                }
            }

            public string FullPath { get; }

            public IList<BindingRedirectModel> RedirectModels { get; private set; }

            public AppConfigFilePropabilityMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability, string fullPath)
                : base(sample, match, probability)
            {
                FullPath = fullPath;
                RedirectModels = ReadBindings(fullPath);
            }

            private IList<BindingRedirectModel> ReadBindings(string configFilePath)
            {
                var doc = new XmlDocument();
                doc.Load(configFilePath);

                var manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("bindings", "urn:schemas-microsoft-com:asm.v1");

                var root = doc.DocumentElement;
                var nodes = root.SelectNodes("/configuration/runtime/bindings:assemblyBinding/bindings:dependentAssembly", manager);
                var result = new List<BindingRedirectModel>();
                for(var i = 0; i < nodes.Count; ++i)
                {
                    var node = nodes.Item(i);
                    var assemblyNameNode = node.SelectSingleNode("bindings:assemblyIdentity", manager);
                    var assemblyRedirectNode = node.SelectSingleNode("bindings:bindingRedirect", manager);

                    var nameNodeAttributeCollection = assemblyNameNode.Attributes;
                    var identity = nameNodeAttributeCollection["name"].Value + " " + nameNodeAttributeCollection["publicKeyToken"].Value + " " + nameNodeAttributeCollection["culture"].Value;

                    var redirectNodeAttributeCollection = assemblyRedirectNode.Attributes;
                    var version = redirectNodeAttributeCollection["newVersion"].Value;

                    result.Add(new BindingRedirectModel(identity, version));
                }

                return result;
            }
        }

    }

    public class NugetPackageFileMatcher : ProbabilityMatch<ProjectItemInstance>
    {
        private const string CONST_ITEM_TYPE_NONE = "None";

        private const string CONST_METADATA_NAME_FILENAME = "Filename";

        private const string CONST_METADATA_NAME_IDENTITY = "Identity";

        private const string CONST_FILENAME_PACKAGES_CONFIG = "packages.config";

        private const string CONST_METADATA_NAME_FULLPATH = "FullPath";

        public override ProbabilityMatchMetadata<ProjectItemInstance> CalculateProbability(ProjectItemInstance dataSample)
        {
            if (!string.Equals(dataSample.ItemType, CONST_ITEM_TYPE_NONE))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFilename = dataSample.MetadataNames.Any(x => string.Equals(x, CONST_METADATA_NAME_FILENAME));
            if (!hasFilename)
            {
                return base.CalculateProbability(dataSample);
            }

            var filename = dataSample.GetMetadataValue(CONST_METADATA_NAME_IDENTITY);
            if (string.IsNullOrEmpty(filename) || !string.Equals(filename, CONST_FILENAME_PACKAGES_CONFIG))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFullPath = dataSample.MetadataNames.Any(x => string.Equals(x, CONST_METADATA_NAME_FULLPATH));
            if (!hasFullPath)
            {
                return base.CalculateProbability(dataSample);
            }

            var fullPath = dataSample.GetMetadataValue(CONST_METADATA_NAME_FULLPATH);
            if (string.IsNullOrEmpty(fullPath))
            {
                return base.CalculateProbability(dataSample);
            }

            return new NugetPackageFilePropabilityMetadata(dataSample, this, 1d, fullPath);
        }

        public class NugetPackageFilePropabilityMetadata : SomeProbabilityMatchMetadata<ProjectItemInstance>
        {
            public string FullPath { get; }

            public NugetPackageFilePropabilityMetadata(ProjectItemInstance sample, ProbabilityMatch<ProjectItemInstance> match, double probability, string fullPath)
                : base(sample, match, probability)
            {
                FullPath = fullPath;
            }
        }
    }
}