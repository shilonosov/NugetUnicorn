namespace NugetUnicorn.Utils.Sax
{
    public abstract class SaxEvent
    {
        public string[] Path { get; }

        protected SaxEvent(string[] path)
        {
            Path = path;
        }
    }
}