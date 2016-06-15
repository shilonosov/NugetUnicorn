using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.SolutionFileParsers
{
    public class AppConfigFileReferenceMatcher : ProbabilityMatch<ProjectItem>
    {
        private const string CONST_ITEM_TYPE_NONE = "None";

        private const string CONST_METADATA_NAME_FILENAME = "Filename";

        private const string CONST_METADATA_NAME_IDENTITY = "Identity";

        private const string CONST_FILENAME_PACKAGES_CONFIG = "app.config";

        private const string CONST_METADATA_NAME_FULLPATH = "FullPath";

        public override ProbabilityMatchMetadata<ProjectItem> CalculateProbability(ProjectItem dataSample)
        {
            if (!string.Equals(dataSample.ItemType, CONST_ITEM_TYPE_NONE))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFilename = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_METADATA_NAME_FILENAME));
            if (!hasFilename)
            {
                return base.CalculateProbability(dataSample);
            }

            var filename = dataSample.GetMetadataValue(CONST_METADATA_NAME_IDENTITY);
            if (string.IsNullOrEmpty(filename) || !string.Equals(filename, CONST_FILENAME_PACKAGES_CONFIG, StringComparison.InvariantCultureIgnoreCase))
            {
                return base.CalculateProbability(dataSample);
            }

            var hasFullPath = dataSample.Metadata.Any(x => string.Equals(x.Name, CONST_METADATA_NAME_FULLPATH));
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

        public class AppConfigFilePropabilityMetadata : SomeProbabilityMatchMetadata<ProjectItem>
        {
            public class BindingRedirectModel
            {
                public string Name { get; private set; }
                public string NewVersion { get; private set; }
                public string PublicKeyToken { get; private set; }
                public string Culture { get; private set; }

                public BindingRedirectModel(string name, string newVersion, string publicKeyToken, string culture)
                {
                    Name = name;
                    NewVersion = newVersion;
                    PublicKeyToken = publicKeyToken;
                    Culture = culture;
                }

                public override string ToString()
                {
                    return $"{Name}, {NewVersion}, {PublicKeyToken}, {Culture}";
                }
            }

            public string FullPath { get; }

            public IList<BindingRedirectModel> RedirectModels { get; private set; }

            public AppConfigFilePropabilityMetadata(ProjectItem sample, ProbabilityMatch<ProjectItem> match, double probability, string fullPath)
                : base(sample, match, probability)
            {
                FullPath = fullPath;
                RedirectModels = ReadBindings(fullPath);
            }

            private IList<BindingRedirectModel> ReadBindings(string configFilePath)
            {
                var result = new List<BindingRedirectModel>();

                var doc = new XmlDocument();
                try
                {
                    doc.Load(configFilePath);
                }
                catch
                {
                    return result;
                }

                var manager = new XmlNamespaceManager(doc.NameTable);
                manager.AddNamespace("bindings", "urn:schemas-microsoft-com:asm.v1");

                var root = doc.DocumentElement;
                if (root == null)
                {
                    return result;
                }

                var nodes = root.SelectNodes("/configuration/runtime/bindings:assemblyBinding/bindings:dependentAssembly", manager);
                if (nodes == null)
                {
                    return result;
                }

                for(var i = 0; i < nodes.Count; ++i)
                {
                    var node = nodes.Item(i);
                    if (node == null)
                    {
                        continue;
                    }

                    var assemblyNameNode = node.SelectSingleNode("bindings:assemblyIdentity", manager);
                    var assemblyRedirectNode = node.SelectSingleNode("bindings:bindingRedirect", manager);

                    var name = assemblyNameNode.GetAttribute("name", string.Empty);
                    var publicKeyToken = assemblyNameNode.GetAttribute("publicKeyToken", string.Empty);
                    var culture = assemblyNameNode.GetAttribute("culture", string.Empty);
                    var version = assemblyRedirectNode.GetAttribute("newVersion", string.Empty);

                    result.Add(new BindingRedirectModel(name, version, publicKeyToken, culture));
                }

                return result;
            }
        }

    }
}