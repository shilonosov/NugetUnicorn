using System;

using NuGet;

namespace NugetUnicorn.Business.Extensions
{
    public static class NugetExtensions
    {
        public static VersionSpec Intersect(this VersionSpec thisVersion, VersionSpec otherVersion)
        {
            if (thisVersion == null || otherVersion == null)
            {
                return null;
            }

            var thisMax = thisVersion.MaxVersion;
            var thisMin = thisVersion.MinVersion;
            var otherMax = otherVersion.MaxVersion;
            var otherMin = otherVersion.MinVersion;

            if ((thisMax != null) && (otherMax != null) && ((thisMax.CompareTo(otherMin) < 0) || otherMax.CompareTo(thisMin) < 0))
            {
                return new VersionSpec();
            }

            Func<int, SemanticVersion, SemanticVersion, SemanticVersion> newMaxPicker = (comparationResult, first, second) => comparationResult < 0 ? first : second;
            Func<int, SemanticVersion, SemanticVersion, SemanticVersion> newMinPicker = (comparationResult, first, second) => comparationResult > 0 ? first : second;

            var newMaxVersion = PickVersion(thisMax, otherMax, newMaxPicker);
            var newMinVersion = PickVersion(thisMin, otherMin, newMinPicker);

            var minVersionInclusive = ((thisMin != null && thisVersion.IsMinInclusive) || (thisMin == null)) && ((otherMin != null && otherVersion.IsMinInclusive) || (otherMin == null));
            var maxVersionInclusive = ((thisMax != null && thisVersion.IsMaxInclusive) || (thisMax == null)) && ((otherMax != null && otherVersion.IsMaxInclusive) || (otherMax == null));

            return new VersionSpec()
                       {
                           MaxVersion = newMaxVersion,
                           MinVersion = newMinVersion,
                           IsMinInclusive = minVersionInclusive,
                           IsMaxInclusive = maxVersionInclusive
                       };
        }

        private static SemanticVersion PickVersion(SemanticVersion first, SemanticVersion second, Func<int, SemanticVersion, SemanticVersion, SemanticVersion> versionPickerFunc)
        {
            if (first == null && second == null)
            {
                return null;
            }
            if (first == null)
            {
                return second;
            }
            if (second == null)
            {
                return first;
            }
            return versionPickerFunc(first.CompareTo(second), first, second);
        }
    }
}