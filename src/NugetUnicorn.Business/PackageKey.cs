using System.IO;

using NuGet;

namespace NugetUnicorn.Business
{
    public class PackageKey
    {
        public const string FileExtension = "json";

        public string Version { get; set; }

        public string Id { get; set; }

        public string FileName => string.IsNullOrEmpty(Version) ? null : (Version + "." + FileExtension);

        public string FullPath => string.IsNullOrEmpty(Version) ? Id : Path.Combine(Id, FileName);

        public PackageKey()
        {
        }

        public PackageKey(IPackage package) : this(package.Id, package.Version.ToString())
        {
        }

        public PackageKey(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public PackageKey(string id) : this(id, null)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as PackageKey;
            return other != null && string.Equals(FullPath, other.FullPath);
        }

        protected bool Equals(PackageKey other)
        {
            return string.Equals(FullPath, other.FullPath);
        }

        public override int GetHashCode()
        {
            return FullPath?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return Id + " " + (Version ?? "no specific version");
        }
    }
}