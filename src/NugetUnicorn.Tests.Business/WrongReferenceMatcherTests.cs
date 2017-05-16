using NugetUnicorn.Business.FuzzyMatcher.Matchers.Analyzer;
using NugetUnicorn.Business.FuzzyMatcher.Matchers.Exceptions;
using NugetUnicorn.Business.SourcesParser.ProjectParser;

using NUnit.Framework;

using Rhino.Mocks;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class WrongReferenceMatcherTests
    {
        [Test]
        public void TestNonUniqueProjectOutputIsWarned()
        {
            const string projectName1 = "project1";
            const string projectName2 = "project2";
            const string projectOutput1 = "output1";

            var projectPocos = new[]
                                   {
                                       ConfigureStub(projectOutput1, projectName1),
                                       ConfigureStub(projectOutput1, projectName2),
                                       ConfigureStub("output3", "project3")
                                   };

            try
            {
                var sut = new WrongReferenceMatcher(projectPocos);
            }
            catch (DuplicateProjectOutputNameException e)
            {
                Assert.AreEqual($"Projects [{projectName1}, {projectName2}] has the same output name: [{projectOutput1}]", e.Message);
            }
        }

        private static IProjectPoco ConfigureStub(string projectOutput, string projectName)
        {
            var stubProjectPoco = MockRepository.GenerateStub<IProjectPoco>();
            stubProjectPoco.Expect(x => x.TargetName).Return(projectOutput);
            stubProjectPoco.Expect(x => x.Name).Return(projectName);
            return stubProjectPoco;
        }
    }
}