using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser;

using NUnit.Framework;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class SaxParserTests
    {
        [Test]
        public void TestParseAttributes()
        {
            const string Xml = @"<root><topNode id=""1""><bottomNode name=""name"">text</bottomNode></topNode></root>";
            var sut = new SaxParser();

            using (var textReader = new StringReader(Xml))
            {
                var events = sut.Parse(textReader, Scheduler.CurrentThread)
                                .ToList()
                                .Wait();
                Assert.AreEqual(6, events.Count);
            }
        }
    }
}