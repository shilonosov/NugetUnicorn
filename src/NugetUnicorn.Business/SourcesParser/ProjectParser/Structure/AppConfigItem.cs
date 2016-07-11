namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class AppConfigItem : ProjectStructureItem
    {
        public string RelativePath { get; private set; }

        public AppConfigItem(string relativePath)
        {
            RelativePath = relativePath;
        }
    }

    public class PackagesConfigItem : ProjectStructureItem
    {
        public string RelativePath { get; private set; }

        public PackagesConfigItem(string relativePath)
        {
            RelativePath = relativePath;
        }
    }
}