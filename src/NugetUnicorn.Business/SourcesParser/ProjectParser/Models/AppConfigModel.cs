using System.Collections.Generic;
using System.Linq;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Models
{
    public class AppConfigModel
    {
        public BindingRedirectModel[] Bindings { get; }

        public AppConfigModel(IEnumerable<BindingRedirectModel> bindings)
        {
            Bindings = bindings.ToArray();
        }
    }
}