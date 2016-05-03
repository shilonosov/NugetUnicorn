using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

using GraphX.PCL.Common.Models;

using NugetUnicorn.Business;

namespace NugetUnicorn.Ui.Business
{
    public class DataVertex : VertexBase
    {
        private readonly string _packageId;
        private readonly IList<PackageKey> _versions;

        private readonly VersionSpecRangeBuilder _versionSpecRangeBuilder;

        public void AddVersion(IList<PackageKey> existing, PackageKey packageKey)
        {
            _versions.Add(packageKey);
            var composed = _versionSpecRangeBuilder.ComposeFrom(existing, _versions)
                                                   .Select(x => x.ToString());
            Text = _packageId + " " + string.Join(", ", composed);
        }

        public string Text { get; private set; }

        public DataVertex(string packageId)
        {
            _packageId = packageId;
            _versionSpecRangeBuilder = new VersionSpecRangeBuilder();
            _versions = new List<PackageKey>();
            Text = _packageId;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}