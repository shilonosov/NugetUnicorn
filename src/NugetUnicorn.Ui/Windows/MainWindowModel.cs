using System.Collections.Generic;
using System.Linq;

using NugetUnicorn.Business;
using NugetUnicorn.Ui.Controls;

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
}