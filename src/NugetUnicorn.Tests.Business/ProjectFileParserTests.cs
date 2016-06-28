using System.IO;
using System.Linq;
using System.Reactive.Linq;

using NugetUnicorn.Business.SourcesParser.ProjectParser;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

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
                                      .OfType<ReferenceBase>()
                                      .ToList()
                                      .Wait();

            Assert.AreEqual(32, parsedReferences.Count);
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
            Assert.AreEqual(@"..\packages\GraphX.2.3.3.0\lib\net40-client\GraphX.Controls.dll", first.HintPath);
        }

        [Test]
        public void TestProjectReferencePath()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath)
                                      .OfType<ProjectReference>()
                                      .ToList()
                                      .Wait();

            var first = parsedReferences.First();
            Assert.AreEqual(1, parsedReferences.Count);
            Assert.AreEqual(@"..\NugetUnicorn.Business\NugetUnicorn.Business.csproj", first.Include);
            Assert.AreEqual(@"NugetUnicorn.Business", first.Name);
            Assert.AreEqual(@"{79721f58-a1bd-4b03-8958-2560eb16d1ad}", first.Guid);
        }
    }
}