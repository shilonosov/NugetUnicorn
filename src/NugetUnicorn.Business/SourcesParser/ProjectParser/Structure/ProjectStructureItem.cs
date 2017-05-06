using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Sax;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class Context
    {
        private readonly IDictionary<int, object> _contextDictionary;

        public Context()
        {
            _contextDictionary = new Dictionary<int, object>();
        }

        public void Set<T>(int key, T value)
        {
            _contextDictionary[key] = value;
        }

        public bool Get<T>(int key, ref T value)
        {
            if (_contextDictionary.ContainsKey(key))
            {
                value = (T)_contextDictionary[key];
                return true;
            }
            return false;
        }
    }

    public interface ICaseSwitch<in T, TV>
    {
        bool Handle(T subject, ref TV value, Context context);
    }

    public class SwitchBatch<T, TV>
    {
        private readonly ICaseSwitch<T, TV>[] _cases;

        private readonly IScheduler _scheduler;

        public SwitchBatch(ICaseSwitch<T, TV>[] cases, IScheduler scheduler = null)
        {
            _cases = cases;
            _scheduler = scheduler ?? CurrentThreadScheduler.Instance;
        }

        public TV Evaluate(T subject)
        {
            var context = new Context();
            return _cases.ToObservable()
                         .ObserveOn(_scheduler)
                         .Select(x => EvaluateInternal(subject, x, context))
                         .FirstOrDefaultAsync(x => x.Item1)
                         .Wait()
                         .Item2;
        }

        private static Tuple<bool, TV> EvaluateInternal(T subject, ICaseSwitch<T, TV> x, Context context)
        {
            var value = default(TV);
            var matched = x.Handle(subject, ref value, context);
            return new Tuple<bool, TV>(matched, value);
        }
    }

    public class ProjectStructureItem
    {
        private const string REFERENCE = "Reference";

        private const string PROJECT_REFERENCE = "ProjectReference";

        private const string APP_CONFIG_NAME = "App.config";

        private const string PACKAGES_CONFIG_NAME = "packages.config";

        private const string INCLUDE_TAG_NAME = "Include";

        private const string NONE_TAG_NAME = "None";

        private const string ITEMGROUP_TAG_NAME = "ItemGroup";

        public static IEnumerable<ProjectStructureItem> Build(CompositeSaxEvent saxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var endElementEvents = descendants?.OfType<EndElementEvent>()
                                              .ToArray();

            return saxEvent.Switch<CompositeSaxEvent, ProjectStructureItem>()
                           .Case(IsReference, x => HandleReference(x, endElementEvents))
                           .Case(IsProjectReference, x => HandleProjectReference(x, endElementEvents))
                           .Case(IsAssemblyName, x => HandleAssemblyName(x, descendants))
                           .Case(IsOutputType, x => HandleOutputType(x, descendants))
                           .Case(x => IsAppConfig(x, descendants), x => HandleAppConfig(x, descendants))
                           .Case(x => IsPackagesConfig(x, descendants), x => HandlePackagesConfig(x, descendants))
                           .Case(IsTargetFrameworkVersion, x => HandleTargetFrameworkVersion(x, descendants))
                           .EvaluateAll();
        }

        private static ProjectStructureItem HandleTargetFrameworkVersion(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var value = descendants.OfType<StringElementEvent>()
                                   .FirstOrDefault()
                                   ?.Content;
            return new TargetFramework(value);
        }

        private static bool IsTargetFrameworkVersion(CompositeSaxEvent compositeSaxEvent)
        {
            return compositeSaxEvent.Path.SequenceEqual(new[] { "Project", "PropertyGroup", "TargetFrameworkVersion" });
        }

        private static ProjectStructureItem HandlePackagesConfig(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var item = descendants?.OfType<EndElementEvent>()
                                  .Where(x => string.Equals(x.Name, NONE_TAG_NAME))
                                  .Where(x => x.Attributes.ContainsKey(INCLUDE_TAG_NAME))
                                  .Select(x => x.Attributes[INCLUDE_TAG_NAME])
                                  .FirstOrDefault(x => string.Equals("packages.config", x, StringComparison.InvariantCultureIgnoreCase));
            return new PackagesConfigItem(item);
        }

        private static bool IsPackagesConfig(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            return HasInclude(compositeSaxEvent, descendants, PACKAGES_CONFIG_NAME);
        }

        private static bool IsAppConfig(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            return HasInclude(compositeSaxEvent, descendants, APP_CONFIG_NAME);
        }

        private static ProjectStructureItem HandleAppConfig(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants)
        {
            var item = descendants?.OfType<EndElementEvent>()
                                  .Where(x => string.Equals(x.Name, NONE_TAG_NAME))
                                  .Where(x => x.Attributes.ContainsKey(INCLUDE_TAG_NAME))
                                  .Select(x => x.Attributes[INCLUDE_TAG_NAME])
                                  .FirstOrDefault(x => string.Equals("app.config", x, StringComparison.InvariantCultureIgnoreCase));
            return new AppConfigItem(item);
        }

        private static bool HasInclude(CompositeSaxEvent compositeSaxEvent, IReadOnlyCollection<SaxEvent> descendants, string includeName)
        {
            var isItemGroup = string.Equals(ITEMGROUP_TAG_NAME, compositeSaxEvent.Name);
            if (!isItemGroup)
            {
                return false;
            }

            var hasInclude = descendants?.OfType<EndElementEvent>()
                                        .Where(x => string.Equals(x.Name, NONE_TAG_NAME))
                                        .Where(x => x.Attributes.ContainsKey(INCLUDE_TAG_NAME))
                                        .Select(x => x.Attributes[INCLUDE_TAG_NAME])
                                        .Any(x => string.Equals(includeName, x, StringComparison.InvariantCultureIgnoreCase));

            return hasInclude.HasValue && hasInclude.Value;
        }

        private static bool IsOutputType(CompositeSaxEvent x)
        {
            return string.Equals(x.Name, "OutputType");
        }

        private static bool IsAssemblyName(CompositeSaxEvent x)
        {
            return string.Equals(x.Name, "AssemblyName");
        }

        private static bool IsProjectReference(CompositeSaxEvent x)
        {
            return string.Equals(x.Name, PROJECT_REFERENCE);
        }

        private static bool IsReference(CompositeSaxEvent x)
        {
            return string.Equals(x.Name, REFERENCE);
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
            var include = saxEvent.Attributes[INCLUDE_TAG_NAME];
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
            var include = saxEvent.Attributes[INCLUDE_TAG_NAME];
            var hintPath = endElementEvents?.SingleOrDefault(x => string.Equals(x.Name, "HintPath"))
                                           ?.Descendants
                                           ?.OfType<StringElementEvent>()
                                           .SingleOrDefault()
                                           ?.Content;
            var isPrivate = endElementEvents?.SingleOrDefault(x => string.Equals(x.Name, "Private"))
                                            ?.Descendants
                                            ?.OfType<StringElementEvent>()
                                            .SingleOrDefault()
                                            ?.Content
                                            ?.ToUpper();

            return new Reference(include, hintPath, string.Equals(true.ToString().ToUpper(), isPrivate));
        }

        public static IEnumerable<ProjectStructureItem> Build(EndElementEvent saxEvent)
        {
            return Build(saxEvent, null);
        }
    }
}