using System.IO;
using System.Reactive.Linq;

using NugetUnicorn.Business.SourcesParser.ProjectParser;

using NUnit.Framework;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class ProjectFileParserTests
    {
        [Test]
        public void TestParseReferencesCount()
        {
            var sut = new ProjectFileParser();

            var fullPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestSamples\SampleProject.xml");
            var parsedReferences = sut.Parse(fullPath)
                                      .OfType<Reference>()
                                      .ToList()
                                      .Wait();

            Assert.AreEqual(18, parsedReferences.Count);
        }
    }
}