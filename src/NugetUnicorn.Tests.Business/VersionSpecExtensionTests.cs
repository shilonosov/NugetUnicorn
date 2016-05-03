using NugetUnicorn.Business;
using NugetUnicorn.Business.Extensions;

using NuGet;

using NUnit.Framework;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class VersionSpecExtensionTests
    {
        [TestCase("1.0.0", true, "2.0.0", true, "1.5.0", true, "2.0.0", false, "[1.5.0, 2.0.0)")]
        [TestCase("3.3.1", true, "3.3.1", true, "3.3.0", true, "3.3.0", true, "(, )")]
        public void IntersectTest(string min1, bool min1Inc, string max1, bool max1Inc, string min2, bool min2Inc, string max2, bool max2Inc, string expected)
        {
            var v1 = CreateVersionSpec(min1, max1, min1Inc, max1Inc);
            var v2 = CreateVersionSpec(min2, max2, min2Inc, max2Inc);
            var intersect = v1.Intersect(v2);

            Assert.AreEqual(expected, intersect.ToString());
        }

        [Test]
        public void IntersectWithNoRestrictionsTest()
        {
            var v1 = CreateVersionSpec("1.0", "2.0", true, true);
            var v2 = new VersionSpec
                         {
                             MinVersion = new SemanticVersion("1.5"),
                             IsMinInclusive = true,
                             MaxVersion = null,
                             IsMaxInclusive = false
                         };
            var intersect = v1.Intersect(v2);

            Assert.AreEqual("[1.5, 2.0]", intersect.ToString());
        }

        private static VersionSpec CreateVersionSpec(string minVersion, string maxVersion, bool isMinInclusive, bool isMaxInclusive)
        {
            return new VersionSpec
                       {
                           MinVersion = new SemanticVersion(minVersion),
                           MaxVersion = new SemanticVersion(maxVersion),
                           IsMinInclusive = isMinInclusive,
                           IsMaxInclusive = isMaxInclusive
                       };
        }
    }
}