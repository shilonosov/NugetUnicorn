using System.Collections.Generic;

namespace NugetUnicorn.Utils.Sax.Parser
{
    public class ContentHolder
    {
        private readonly List<SaxEvent> _content;

        public StartElementEvent StartSaxEvent { get; }

        public ContentHolder Parent { get; }

        public ContentHolder()
            : this(null)
        {
        }

        public ContentHolder(StartElementEvent startSaxEvent)
            : this(startSaxEvent, null)
        {
        }

        public ContentHolder(StartElementEvent startSaxEvent, ContentHolder parent)
        {
            _content = new List<SaxEvent>();
            Parent = parent;
            StartSaxEvent = startSaxEvent;
        }

        public void Append(SaxEvent contentItem)
        {
            _content.Add(contentItem);
        }

        public IReadOnlyCollection<SaxEvent> GetContent()
        {
            return _content.AsReadOnly();
        }
    }
}