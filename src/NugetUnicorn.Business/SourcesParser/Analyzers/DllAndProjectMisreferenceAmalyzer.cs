using System.Collections.Generic;
using System.Linq;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.Metadata;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher.ReferenceType;
using NugetUnicorn.Dto;
using NugetUnicorn.Utils.Extensions;
using NugetUnicorn.Utils.FuzzyMatcher.Engine;

namespace NugetUnicorn.Business.SourcesParser.Analyzers
{
    public class DllAndProjectMisreferenceAmalyzer
    {
        public static IDictionary<ProjectPoco, IEnumerable<string>> ComposeProjectReferenceErrors(
            IDictionary<ProjectPoco, IEnumerable<ReferenceMetadataBase>> referenceMetadatas,
            IProbabilityMatchEngine<DllMetadata> wrongReferenceMatcher)
        {
            return referenceMetadatas.Transform(x => x.OfType<DllMetadata>())
                                     .Transform(
                                         x =>
                                             x.FindBestMatch<DllMetadata, WrongReferencePropabilityMetadata>(wrongReferenceMatcher, 0d))
                                     .Transform(
                                         x => x.Select(
                                             y =>
                                                 $"found possible misreference: {y.Reference} (solution contains project with the same target name: {y.SuspectedProject.Name} / {y.SuspectedProject.TargetName})"));
        }
    }
}