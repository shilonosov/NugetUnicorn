using System.Collections.Generic;
using NugetUnicorn.Dto.Structure;
using NugetUnicorn.Utils.Sax;
using NugetUnicorn.Utils.Sax.Parser;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
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