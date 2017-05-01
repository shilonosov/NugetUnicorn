namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public class StringElementEvent : SaxEvent
    {
        public string Content { get; }

        public StringElementEvent(string content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}