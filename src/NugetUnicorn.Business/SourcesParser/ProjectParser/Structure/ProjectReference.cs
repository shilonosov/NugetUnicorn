namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class ProjectReference : ReferenceBase
    {
        public string Name { get; }

        public string Guid { get; }

        public ProjectReference(string include, string name, string guid)
            : base(include)
        {
            Name = name;
            Guid = guid;
        }
    }
}