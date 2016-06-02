using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using NugetUnicorn.Business;
using NugetUnicorn.Ui.Controls;

using Reactive.Bindings;

namespace NugetUnicorn.Ui.Windows
{
    public class MainWindowModel
    {
        public IList<PackageControlModel> PackageKeys { get; }

        public MainWindowModel(INugetLibraryProxy nugetLibraryProxy, IEnumerable<PackageKey> packageKeys)
        {
            PackageKeys = packageKeys.Select(x => new PackageControlModel(x, nugetLibraryProxy.GetById(x.Id).Select(y => y.Key)))
                                     .ToList();
        }
    }

    public class MainWindowViewModel
    {
        public ReactiveCollection<PackageControlViewModel> Packages { get; private set; }

        public ReactiveProperty<string> SelectedSolutionProperty { get; private set; }

        public ReactiveCommand SelectSolutionCommand { get; private set; }

        public MainWindowViewModel(MainWindowModel model)
        {
            Packages = model.PackageKeys
                            .Select(x => new PackageControlViewModel(x))
                            .ToObservable()
                            .ToReactiveCollection();

            SelectedSolutionProperty = new ReactiveProperty<string>();

            SelectSolutionCommand = new ReactiveCommand();
            SelectSolutionCommand.Subscribe(x => SelectedSolutionProperty.Value = "ololo");
        }
    }
}
