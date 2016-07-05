namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class AssemblyName : ProjectStructureItem
    {
        public string Name { get; private set; }

        public AssemblyName(string name)
        {
            Name = name;
        }
    }
}