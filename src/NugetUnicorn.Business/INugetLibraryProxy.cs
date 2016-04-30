using System.Collections.Generic;

namespace NugetUnicorn.Business
{
    public interface INugetLibraryProxy
    {
        IEnumerable<PackageDto> GetById(string id);

        PackageDto GetByKey(PackageKey key);
    }
}