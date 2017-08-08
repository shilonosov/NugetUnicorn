namespace NugetUnicorn.Dto.Structure
{
    public class Reference : ReferenceBase
    {
        public bool IsPrivate { get; private set; }

        public string HintPath { get; private set; }

        public Reference(string include, string hintPath, bool isPrivate)
            : base(include)
        {
            IsPrivate = isPrivate;
            HintPath = hintPath;
        }
    }
}