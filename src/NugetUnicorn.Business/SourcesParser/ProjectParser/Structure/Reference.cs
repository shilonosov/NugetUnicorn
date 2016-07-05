namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
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