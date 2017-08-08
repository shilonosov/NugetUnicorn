using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Evaluation;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Models;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.SolutionFileParsers
{
    public class AppConfigFileReferenceMatcher : ProbabilityMatch<ProjectItem>
    {
        private const string CONST_ITEM_TYPE_NONE = "None";

        private const string CONST_METADATA_NAME_FILENAME = "Filename";

        private const string CONST_METADATA_NAME_IDENTITY = "Identity";

        private const string CONST_FILENAME_PACKAGES_CONFIG = "app.config";

        private const string CONST_METADATA_NAME_FULLPATH = "FullPath";

        private readonly IAppConfigParser _appConfigParser;

        public AppConfigFileReferenceMatcher(IAppConfigParser appConfigParser)
        {
            _appConfigParser = appConfigParser;
        }

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

            return new AppConfigFilePropabilityMetadata(dataSample, this, 1d, fullPath, _appConfigParser.ReadBindings(fullPath));
        }

        // TODO: review, refactor
        public class AppConfigFilePropabilityMetadata : SomeProbabilityMatchMetadata<ProjectItem>
        {
            public string FullPath { get; }

            public IList<BindingRedirectModel> RedirectModels { get; }

            public AppConfigFilePropabilityMetadata(ProjectItem sample,
                                                    ProbabilityMatch<ProjectItem> match,
                                                    double probability,
                                                    string fullPath,
                                                    IList<BindingRedirectModel> redirectModels)
                : base(sample, match, probability)
            {
                FullPath = fullPath;
                RedirectModels = redirectModels;
            }
        }
    }
}