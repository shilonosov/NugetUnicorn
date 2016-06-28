using System.Collections.Generic;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public class EndElementEvent : CompositeSaxEvent
    {
        public IReadOnlyCollection<SaxEvent> Descendants { get; }

        public EndElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes, IReadOnlyCollection<SaxEvent> descendants)
            : base(uri, name, isClosed, attributes)
        {
            Descendants = descendants;
        }
    }
}