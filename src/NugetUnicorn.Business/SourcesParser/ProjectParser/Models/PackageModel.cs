namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Models
{
    public class PackageModel
    {
        public string Id { get; }
        public string Version { get; }
        public string TargetFramework { get; }

        public PackageModel(string id, string version, string targetFramework)
        {
            Id = id;
            Version = version;
            TargetFramework = targetFramework;
        }
    }
}