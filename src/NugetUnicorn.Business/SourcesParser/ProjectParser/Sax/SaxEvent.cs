namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public abstract class SaxEvent
    {
        public string[] Path { get; }

        protected SaxEvent(string[] path)
        {
            Path = path;
        }
    }
}