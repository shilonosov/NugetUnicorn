using System.Collections.Generic;

namespace NugetUnicorn.Utils.Sax
{
    public class EndElementEvent : CompositeSaxEvent
    {
        public IReadOnlyCollection<SaxEvent> Descendants { get; }

        public EndElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes, IReadOnlyCollection<SaxEvent> descendants, string[] path)
            : base(uri, name, isClosed, attributes, path)
        {
            Descendants = descendants;
        }
    }
}