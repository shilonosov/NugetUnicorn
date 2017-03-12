namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class AssemblyName : ProjectStructureItem
    {
        public string Name { get; }

        public AssemblyName(string name)
        {
            Name = name;
        }
    }
}