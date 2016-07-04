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

    public class OutputType : ProjectStructureItem
    {
        public string Content { get; private set; }

        public OutputType(string outputType)
        {
            Content = outputType;
        }
    }

    public abstract class ReferenceBase : ProjectStructureItem
    {
        public string Include { get; private set; }

        protected ReferenceBase(string include)
        {
            Include = include;
        }
    }

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

    public class Reference : ReferenceBase
    {
        public string HintPath { get; private set; }

        public Reference(string include, string hintPath)
            : base(include)
        {
            HintPath = hintPath;
        }
    }
}