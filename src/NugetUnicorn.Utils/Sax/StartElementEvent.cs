using System.Collections.Generic;

namespace NugetUnicorn.Utils.Sax
{
    public class StartElementEvent : CompositeSaxEvent
    {
        public StartElementEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes, string[] path)
            : base(uri, name, isClosed, attributes, path)
        {
        }
    }
}