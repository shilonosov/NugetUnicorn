using System.Collections.Generic;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser
{
    public interface IXmlModelBuilder<out T>
    {
        IEnumerable<T> ComposeElement(StartElementEvent startElement, IReadOnlyCollection<SaxEvent> content);

        IEnumerable<T> ComposeElement(EndElementEvent endElement);
    }
}