using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using NugetUnicorn.Business;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Controls
{
    public class PackageControlModel
    {
        public string Name { get; }
        public string Version { get; }
        public IList<string> AvailableVersions { get; }
        public int FixVersionIndex { get; }

        public PackageControlModel(PackageKey packageKey, IEnumerable<PackageKey> availableVersions)
        {
            Name = packageKey.Id;
            Version = packageKey.Version;
            AvailableVersions = availableVersions.Select(x => x.Version)
                                                 .ToList();
            FixVersionIndex = string.IsNullOrEmpty(Version) ? -1 : AvailableVersions.IndexOf(Version);
        }
    }

    public class PackageControlViewModel
    {
        public ReactiveProperty<string> ControlName { get; }
        public ReactiveCollection<string> Versions { get; }
        public ReactiveProperty<int> SelectedVersionIndex { get; }
        public ReactiveProperty<bool> FixVersion { get; }

        public PackageControlViewModel(PackageControlModel model)
        {
            ControlName = new ReactiveProperty<string>(model.Name);
            Versions = model.AvailableVersions
                            .ToObservable()
                            .ToReactiveCollection();
            SelectedVersionIndex = new ReactiveProperty<int>(model.FixVersionIndex);
            FixVersion = new ReactiveProperty<bool>(model.FixVersionIndex >= 0);
        }
    }
}
