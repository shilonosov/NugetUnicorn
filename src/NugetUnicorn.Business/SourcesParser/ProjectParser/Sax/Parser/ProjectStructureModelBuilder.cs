using System.Collections.Generic;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Structure;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Sax.Parser
{
    public class ProjectStructureModelBuilder : IXmlModelBuilder<ProjectStructureItem>
    {
        public IEnumerable<ProjectStructureItem> ComposeElement(StartElementEvent startElement, IReadOnlyCollection<SaxEvent> content)
        {
            return ProjectStructureItem.Build(startElement, content);
        }

        public IEnumerable<ProjectStructureItem> ComposeElement(EndElementEvent endElement)
        {
            return ProjectStructureItem.Build(endElement);
        }
    }
}