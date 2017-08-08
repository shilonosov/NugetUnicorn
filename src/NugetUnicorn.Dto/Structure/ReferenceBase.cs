namespace NugetUnicorn.Dto.Structure
{
    public abstract class ReferenceBase : ProjectStructureItem
    {
        public string Include { get; }

        protected ReferenceBase(string include)
        {
            Include = include;
        }
    }
}