using System;
using System.IO;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Dto;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata
{
    public abstract class ExistingReferenceMetadataBase : ReferenceMetadataBase
    {
        private readonly string _targetPath;

        protected ExistingReferenceMetadataBase(ReferenceBase sample, ProbabilityMatch<ReferenceBase> match, double probability, string targetPath)
            : base(sample, match, probability)
        {
            _targetPath = targetPath;
        }

        public virtual ReferenceInformation GetReferenceInformation(ProjectPoco projectPoco)
        {
            var fullPath = Path.GetFullPath(Path.IsPathRooted(_targetPath) ? _targetPath : Path.Combine(projectPoco.ProjectFilePath.DirectoryPath, _targetPath));
            try
            {
                return new ReferenceInformation(fullPath);
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Error parsing {projectPoco.Name} -- {fullPath}", e);
            }
        }
    }
}