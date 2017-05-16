namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public class StringElementEvent : SaxEvent
    {
        public string Content { get; }

        public StringElementEvent(string content, string[] path)
            : base(path)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}