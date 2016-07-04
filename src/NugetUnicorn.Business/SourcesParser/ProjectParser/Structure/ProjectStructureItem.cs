using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Sax;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class ProjectStructureItem
    {
        private const string REFERENCE = "Reference";

        private const string PROJECT_REFERENCE = "ProjectReference";

        public static ProjectStructureItem Build(CompositeSaxEvent saxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var endElementEvents = descendants?.OfType<EndElementEvent>()
                                               .ToArray();

            return saxEvent.Switch<CompositeSaxEvent, ProjectStructureItem>()
                           .Case(x => string.Equals(x.Name, REFERENCE), x => HandleReference(x, endElementEvents))
                           .Case(x => string.Equals(x.Name, PROJECT_REFERENCE), x => HandleProjectReference(x, endElementEvents))
                           .Case(x => string.Equals(x.Name, "AssemblyName"), x => HandleAssemblyName(x, descendants))
                           .Case(x => string.Equals(x.Name, "OutputType"), x => HandleOutputType(x, descendants))
                           .Evaluate();
        }

        private static ProjectStructureItem HandleOutputType(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var assemblyName = descendants.OfType<StringElementEvent>()
                                          .Single()
                                          .Content;
            return new OutputType(assemblyName);
        }

        private static ProjectStructureItem HandleAssemblyName(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var assemblyName = descendants.OfType<StringElementEvent>()
                                          .Single()
                                          .Content;
            return new AssemblyName(assemblyName);
        }

        private static ProjectStructureItem HandleProjectReference(CompositeSaxEvent saxEvent, EndElementEvent[] endElementEvents)
        {
            var include = saxEvent.Attributes["Include"];
            var guid = endElementEvents.Single(x => string.Equals(x.Name, "Project"))
                                       .Descendants
                                       .OfType<StringElementEvent>()
                                       .Single()
                                       .Content;
            var name = endElementEvents.Single(x => string.Equals(x.Name, "Name"))
                                       .Descendants
                                       .OfType<StringElementEvent>()
                                       .Single()
                                       .Content;
            return new ProjectReference(include, name, guid);
        }

        private static ProjectStructureItem HandleReference(CompositeSaxEvent saxEvent, EndElementEvent[] endElementEvents)
        {
            var include = saxEvent.Attributes["Include"];
            var hintPath = endElementEvents.Single(x => string.Equals(x.Name, "HintPath"))
                                           .Descendants
                                           .OfType<StringElementEvent>()
                                           .Single()
                                           .Content;
            return new Reference(include, hintPath);
        }

        public static ProjectStructureItem Build(EndElementEvent saxEvent)
        {
            return Build(saxEvent, null);
        }
    }
}