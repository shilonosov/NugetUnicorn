using NugetUnicorn.Business.Extensions;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class OutputType : ProjectStructureItem
    {
        public string Content { get; private set; }

        public string Extension { get; private set; }

        public OutputType(string outputType)
        {
            Content = outputType;

            Extension = outputType.Switch<string, string>()
                                  .Case(x => string.Equals(x, "WinExe"), x => "exe")
                                  .Case(x => string.Equals(x, "Exe"), x => "exe")
                                  .Case(x => string.Equals(x, "Library"), x => "dll")
                                  .Evaluate();
        }
    }
}