using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using NugetUnicorn.Business.Extensions;

using NuGet;

namespace NugetUnicorn.Business
{
    public class VersionSpecRangeBuilder
    {
        public IList<VersionSpec> ComposeFrom(IList<PackageKey> existingVersions, IList<PackageKey> packageKeys)
        {
            var existingEnumerator = existingVersions.OrderBy(x => x.Version)
                                                     .GetEnumerator();
            if (!existingEnumerator.MoveNext())
            {
                return new List<VersionSpec>();
            }

            return packageKeys.OrderBy(x => x.Version)
                              .ToObservable()
                              .Cutted(
                                  x =>
                                      {
                                          if (Equals(x, existingEnumerator.Current))
                                          {
                                              existingEnumerator.MoveNext();
                                              return ObservableExtensions.CutterAction.Continue;
                                          }
                                          do
                                          {
                                              if (!existingEnumerator.MoveNext())
                                              {
                                                  return ObservableExtensions.CutterAction.Break;
                                              }
                                          }
                                          while (!Equals(x, existingEnumerator.Current));
                                          existingEnumerator.MoveNext();
                                          return ObservableExtensions.CutterAction.Skip;
                                      })
                              .Where(x => x.Any())
                              .Select(
                                  x =>
                                      {
                                          var versionSpec = new VersionSpec
                                                                {
                                                                    IsMinInclusive = true,
                                                                    IsMaxInclusive = true,
                                                                    MinVersion = new SemanticVersion(x.First().Version),
                                                                    MaxVersion = new SemanticVersion(x.Last().Version)
                                                                };
                                          return versionSpec;
                                      })
                              .ToList()
                              .Wait();
        }
    }
}