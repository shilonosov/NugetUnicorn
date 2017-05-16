namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class AppConfigItem : ProjectStructureItem
    {
        public string RelativePath { get; }

        public AppConfigItem(string relativePath)
        {
            RelativePath = relativePath;
        }
    }

    public class PackagesConfigItem : ProjectStructureItem
    {
        public string RelativePath { get; }

        public PackagesConfigItem(string relativePath)
        {
            RelativePath = relativePath;
        }
    }
}