namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public abstract class ReferenceBase : ProjectStructureItem
    {
        public string Include { get; private set; }

        protected ReferenceBase(string include)
        {
            Include = include;
        }
    }
}