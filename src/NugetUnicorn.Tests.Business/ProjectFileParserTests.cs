using System.IO;
using System.Linq;
using System.Reactive.Linq;

using NugetUnicorn.Business.SourcesParser.ProjectParser;

using NUnit.Framework;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class ProjectFileParserTests
    {
        private readonly string _fullPath;

        public ProjectFileParserTests()
        {
            _fullPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"TestSamples\SampleProject.xml");
        }

        [Test]
        public void TestParseReferencesCount()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath)
                                      .OfType<Reference>()
                                      .ToList()
                                      .Wait();

            Assert.AreEqual(18, parsedReferences.Count);
        }

        [Test]
        public void TestHintPath()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath)
                                      .OfType<Reference>()
                                      .ToList()
                                      .Wait();

            var first = parsedReferences.First();
            Assert.AreEqual(@"C:\Program Files (x86)\MSBuild\14.0\Bin\Microsoft.Build.dll", first.HintPath);
        }
    }
}