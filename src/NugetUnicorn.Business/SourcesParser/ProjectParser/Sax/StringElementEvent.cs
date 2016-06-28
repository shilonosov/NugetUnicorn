namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public class StringElementEvent : SaxEvent
    {
        public string Content { get; }

        public StringElementEvent(string content) : base()
        {
            Content = content;
        }
    }
}