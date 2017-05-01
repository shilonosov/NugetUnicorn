using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax
{
    public abstract class CompositeSaxEvent : SaxEvent
    {
        public string Uri { get; }

        public string Name { get; }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        public bool IsClosed { get; }

        protected CompositeSaxEvent(string uri, string name, bool isClosed, IReadOnlyDictionary<string, string> attributes)
        {
            Uri = uri;
            Name = name;
            Attributes = attributes;
            IsClosed = isClosed;
        }

        public override string ToString()
        {
            return $"{GetType().Name} {Name}:{Uri}, {nameof(IsClosed)}:{IsClosed}, {string.Join(", ", Attributes.Select(x => $"{x.Key}:{x.Value}"))}";
        }
    }
}