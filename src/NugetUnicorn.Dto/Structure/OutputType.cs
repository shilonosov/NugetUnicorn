using NugetUnicorn.Utils.Extensions;

namespace NugetUnicorn.Dto.Structure
{
    public class OutputType : ProjectStructureItem
    {
        public string Content { get; }

        public string Extension { get; }

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