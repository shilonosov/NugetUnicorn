namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Models
{
    public class BindingRedirectModel
    {
        public string Name { get; }

        public string NewVersion { get; }

        public string PublicKeyToken { get; }

        public string Culture { get; }

        public BindingRedirectModel(string name, string newVersion, string publicKeyToken, string culture)
        {
            Name = name;
            NewVersion = newVersion;
            PublicKeyToken = publicKeyToken;
            Culture = culture;
        }

        public override string ToString()
        {
            return $"{Name}, {NewVersion}, {PublicKeyToken}, {Culture}";
        }
    }
}