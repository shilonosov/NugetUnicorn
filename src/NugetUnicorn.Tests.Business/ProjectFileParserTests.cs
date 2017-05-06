using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;

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

            var parsedReferences = sut.Parse(_fullPath);

            Assert.AreEqual(32, parsedReferences.References.Count);
        }

        [Test]
        public void TestParsePackagesConfig()
        {
            const string Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <Project>
                                    <ItemGroup>
                                        <None Include=""packages.config"">
                                            <SubType>Designer</SubType>
                                        </None>
                                    </ItemGroup>
                                </Project>";

            using (var textReader = new StringReader(Xml))
            {
                using (var projectStructureItemSubject = new Subject<ProjectStructureItem>())
                {
                    var projectStructureItems = new List<ProjectStructureItem>();
                    projectStructureItemSubject.Subscribe(projectStructureItems.Add);

                    var sut = new ProjectFileParser();
                    sut.Parse(textReader, projectStructureItemSubject);

                    var items = projectStructureItems
                        .OfType<PackagesConfigItem>()
                        .ToList();

                    Assert.AreEqual(1, items.Count);
                }
            }
        }

        [Test]
        public void TestParseReferences()
        {
            const string Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <Project>
                                    <ItemGroup>
                                        <Reference Include=""System.Data"" />
                                        <Reference Include=""System.Reactive.Core, Version=2.2.5.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL"">
                                            <HintPath>..\packages\Rx-Core.2.2.5\lib\net45\System.Reactive.Core.dll</HintPath>
                                        <Private>True</Private>
                                        </Reference>
                                    </ItemGroup>
                                </Project>";

            using (var textReader = new StringReader(Xml))
            {
                using (var projectStructureItemSubject = new Subject<ProjectStructureItem>())
                {
                    var projectStructureItems = new List<ProjectStructureItem>();
                    projectStructureItemSubject.Subscribe(projectStructureItems.Add);

                    var sut = new ProjectFileParser();
                    sut.Parse(textReader, projectStructureItemSubject);

                    var items = projectStructureItems
                        .OfType<ReferenceBase>()
                        .ToList();

                    Assert.AreEqual(2, items.Count);
                }
            }
        }

        [Test]
        public void TestParsePackagesConfig2()
        {
            const string Xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <Project>
                                    <ItemGroup>
                                        <None Include=""app.config"" />
                                        <None Include=""packages.config"" />
                                    </ItemGroup>
                                </Project>";
            using (var textReader = new StringReader(Xml))
            {
                using (var projectStructureItemSubject = new Subject<ProjectStructureItem>())
                {
                    var projectStructureItems = new List<ProjectStructureItem>();
                    projectStructureItemSubject.Subscribe(projectStructureItems.Add);

                    var sut = new ProjectFileParser();
                    sut.Parse(textReader, projectStructureItemSubject);

                    var packagesConfig = projectStructureItems
                        .OfType<PackagesConfigItem>()
                        .ToList();

                    Assert.AreEqual(1, packagesConfig.Count);
                    Assert.AreEqual("packages.config", packagesConfig.First().RelativePath);

                    var appConfig = projectStructureItems
                        .OfType<AppConfigItem>()
                        .ToList();

                    Assert.AreEqual(1, appConfig.Count);
                    Assert.AreEqual("app.config", appConfig.First().RelativePath);
                }
            }
        }

        [Test]
        public void TestHintPath()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath);

            var first = parsedReferences.References.OfType<Reference>().First();
            Assert.NotNull(first);
            Assert.AreEqual(@"..\packages\GraphX.2.3.3.0\lib\net40-client\GraphX.Controls.dll", first.HintPath);
        }

        [Test]
        public void TestProjectReferencePath()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath);

            var references = parsedReferences.References;
            var projectReferences = references.OfType<ProjectReference>()
                                              .ToArray();
            var first = projectReferences.First();

            Assert.NotNull(first);
            Assert.AreEqual(1, projectReferences.Length);
            Assert.AreEqual(@"..\NugetUnicorn.Business\NugetUnicorn.Business.csproj", first.Include);
            Assert.AreEqual(@"NugetUnicorn.Business", first.Name);
            Assert.AreEqual(@"{79721f58-a1bd-4b03-8958-2560eb16d1ad}", first.Guid);
        }

        [Test]
        public void TestProjectTargetName()
        {
            var sut = new ProjectFileParser();

            var parsedReferences = sut.Parse(_fullPath);

            Assert.AreEqual("NugetUnicorn.Ui.exe", parsedReferences.TargetName);
        }

        [Test]
        public void TestParseTargetFrameworkVersion()
        {
            var sut = new ProjectFileParser();

            var projectPoco = sut.Parse(_fullPath);

            Assert.NotNull(projectPoco);
            Assert.AreEqual("net461", projectPoco.TargetFramework.ShortFolderName);
        }
    }
}