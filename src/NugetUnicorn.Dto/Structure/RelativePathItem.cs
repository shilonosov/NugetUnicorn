namespace NugetUnicorn.Dto.Structure
{
    public abstract class RelativePathItem : ProjectStructureItem
    {
        public string RelativePath { get; }

        protected RelativePathItem(string relativePath)
        {
            RelativePath = relativePath;
        }
    }
}