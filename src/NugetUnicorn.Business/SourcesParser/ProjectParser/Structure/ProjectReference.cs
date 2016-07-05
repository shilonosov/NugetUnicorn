namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class ProjectReference : ReferenceBase
    {
        public string Name { get; private set; }

        public string Guid { get; private set; }

        public ProjectReference(string include, string name, string guid)
            : base(include)
        {
            Name = name;
            Guid = guid;
        }
    }
}