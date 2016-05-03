using System.Collections.Generic;

using NugetUnicorn.Business.Dto;

namespace NugetUnicorn.Business
{
    public interface INugetLibraryProxy
    {
        IEnumerable<PackageDto> GetById(string id);

        PackageDto GetByKey(PackageKey key);
    }
}