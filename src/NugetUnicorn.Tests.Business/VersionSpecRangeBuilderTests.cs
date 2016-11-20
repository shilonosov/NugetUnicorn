using System.Collections.Generic;

using NugetUnicorn.Business;

using NUnit.Framework;

namespace NugetUnicorn.Tests.Business
{
    [TestFixture]
    public class VersionSpecRangeBuilderTests
    {
        [Test]
        public void UnionVersionsTest()
        {
            const string ExpectedString = "[1.0, 2.0]";
            const string PackageId = "some package id";

            var existingVersions = new List<PackageKey>
                                       {
                                           new PackageKey(PackageId, "1.0"),
                                           new PackageKey(PackageId, "2.0")
                                       };

            var packageKeys = new List<PackageKey>
                                  {
                                      new PackageKey(PackageId, "1.0"),
                                      new PackageKey(PackageId, "2.0")
                                  };

            var builder = new VersionSpecRangeBuilder();
            var versionSpec = builder.ComposeFrom(existingVersions, packageKeys);

            Assert.AreEqual(1, versionSpec.Count);
            Assert.AreEqual(ExpectedString, versionSpec[0].ToString());
        }

        [Test]
        public void UnionVersionsOrderTest()
        {
            const string ExpectedString = "[1.0, 2.0]";
            const string PackageId = "some package id";

            var existingVersions = new List<PackageKey>
                                       {
                                           new PackageKey(PackageId, "2.0"),
                                           new PackageKey(PackageId, "1.0")
                                       };

            var packageKeys = new List<PackageKey>
                                  {
                                      new PackageKey(PackageId, "2.0"),
                                      new PackageKey(PackageId, "1.0")
                                  };

            var builder = new VersionSpecRangeBuilder();
            var versionSpec = builder.ComposeFrom(existingVersions, packageKeys);

            Assert.AreEqual(1, versionSpec.Count);
            Assert.AreEqual(ExpectedString, versionSpec[0].ToString());
        }

        [Test]
        public void UnionVersionsALotOfExistingTest()
        {
            const string ExpectedString = "[1.0, 2.0]";
            const string PackageId = "some package id";

            var existingVersions = new List<PackageKey>
                                       {
                                           new PackageKey(PackageId, "0.1"),
                                           new PackageKey(PackageId, "1.0"),
                                           new PackageKey(PackageId, "2.0"),
                                           new PackageKey(PackageId, "3.0")
                                       };

            var packageKeys = new List<PackageKey>
                                  {
                                      new PackageKey(PackageId, "1.0"),
                                      new PackageKey(PackageId, "2.0")
                                  };

            var builder = new VersionSpecRangeBuilder();
            var versionSpec = builder.ComposeFrom(existingVersions, packageKeys);

            Assert.AreEqual(1, versionSpec.Count);
            Assert.AreEqual(ExpectedString, versionSpec[0].ToString());
        }

        [Test]
        public void UnionVersionsSkipIntermidiateTest()
        {
            const string ExpectedString1 = "[1.0]";
            const string ExpectedString2 = "[3.0]";
            const string PackageId = "some package id";

            var existingVersions = new List<PackageKey>
                                       {
                                           new PackageKey(PackageId, "1.0"),
                                           new PackageKey(PackageId, "2.0"),
                                           new PackageKey(PackageId, "3.0")
                                       };

            var packageKeys = new List<PackageKey>
                                  {
                                      new PackageKey(PackageId, "1.0"),
                                      new PackageKey(PackageId, "3.0")
                                  };

            var builder = new VersionSpecRangeBuilder();
            var versionSpec = builder.ComposeFrom(existingVersions, packageKeys);

            Assert.AreEqual(2, versionSpec.Count);
            Assert.AreEqual(ExpectedString1, versionSpec[0].ToString());
            Assert.AreEqual(ExpectedString2, versionSpec[1].ToString());
        }

        [Test]
        public void UnionVersionsSkipIntermidiateTest2()
        {
            const string ExpectedString1 = "[1.0, 2.0]";
            const string ExpectedString2 = "[4.0, 5.0]";
            const string PackageId = "some package id";

            var existingVersions = new List<PackageKey>
                                       {
                                           new PackageKey(PackageId, "1.0"),
                                           new PackageKey(PackageId, "2.0"),
                                           new PackageKey(PackageId, "3.0"),
                                           new PackageKey(PackageId, "4.0"),
                                           new PackageKey(PackageId, "5.0")
                                       };

            var packageKeys = new List<PackageKey>
                                  {
                                      new PackageKey(PackageId, "1.0"),
                                      new PackageKey(PackageId, "2.0"),
                                      new PackageKey(PackageId, "4.0"),
                                      new PackageKey(PackageId, "5.0")
                                  };

            var builder = new VersionSpecRangeBuilder();
            var versionSpec = builder.ComposeFrom(existingVersions, packageKeys);

            Assert.AreEqual(2, versionSpec.Count);
            Assert.AreEqual(ExpectedString1, versionSpec[0].ToString());
            Assert.AreEqual(ExpectedString2, versionSpec[1].ToString());
        }
    }
}