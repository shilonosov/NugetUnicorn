using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.SourcesParser.ProjectParser.Sax;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class ProjectStructureItem
    {
        public static ProjectStructureItem Build(CompositeSaxEvent saxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var endElementEvents = descendants?.OfType<EndElementEvent>()
                .ToArray();

            if (string.Equals("Reference", saxEvent.Name))
            {
                var include = saxEvent.Attributes["Include"];
                var hintPath = endElementEvents
                    ?.FirstOrDefault(x => string.Equals(x.Name, "HintPath"))
                    ?.Descendants
                    ?.OfType<StringElementEvent>()
                    ?.FirstOrDefault()
                    ?.Content;
                return new Reference(include, hintPath);
            }
            if (string.Equals("ProjectReference", saxEvent.Name))
            {
                var include = saxEvent.Attributes["Include"];
                var guid = endElementEvents
                    ?.FirstOrDefault(x => string.Equals(x.Name, "Project"))
                    ?.Descendants
                    ?.OfType<StringElementEvent>()
                    ?.FirstOrDefault()
                    ?.Content;
                var name = endElementEvents
                    ?.FirstOrDefault(x => string.Equals(x.Name, "Name"))
                    ?.Descendants
                    ?.OfType<StringElementEvent>()
                    ?.FirstOrDefault()
                    ?.Content;
                return new ProjectReference(include, name, guid);
            }

            return null;
        }

        public static ProjectStructureItem Build(EndElementEvent saxEvent)
        {
            return Build(saxEvent, null);
        }
    }
}