using System.Collections.Generic;
using System.Xml;

using NugetUnicorn.Business.Extensions;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Models;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser
{
    public interface IAppConfigParser
    {
        AppConfigModel Parse(string filePath);

        IList<BindingRedirectModel> ReadBindings(string configFilePath);
    }

    public class AppConfigParser : IAppConfigParser
    {
        public static IAppConfigParser Instance { get; } = new AppConfigParser();

        public AppConfigModel Parse(string filePath)
        {
            var bindings = ReadBindings(filePath);
            return new AppConfigModel(bindings);
        }

        public IList<BindingRedirectModel> ReadBindings(string configFilePath)
        {
            var result = new List<BindingRedirectModel>();

            var doc = new XmlDocument();
            try
            {
                doc.Load(configFilePath);
            }
            catch
            {
                return result;
            }

            var manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("bindings", "urn:schemas-microsoft-com:asm.v1");

            var root = doc.DocumentElement;
            if (root == null)
            {
                return result;
            }

            var nodes = root.SelectNodes("/configuration/runtime/bindings:assemblyBinding/bindings:dependentAssembly", manager);
            if (nodes == null)
            {
                return result;
            }

            for (var i = 0; i < nodes.Count; ++i)
            {
                var node = nodes.Item(i);
                if (node == null)
                {
                    continue;
                }

                var assemblyNameNode = node.SelectSingleNode("bindings:assemblyIdentity", manager);
                var assemblyRedirectNode = node.SelectSingleNode("bindings:bindingRedirect", manager);

                var name = assemblyNameNode.GetAttribute("name", string.Empty);
                var publicKeyToken = assemblyNameNode.GetAttribute("publicKeyToken", string.Empty);
                var culture = assemblyNameNode.GetAttribute("culture", string.Empty);
                var version = assemblyRedirectNode.GetAttribute("newVersion", string.Empty);

                result.Add(new BindingRedirectModel(name, version, publicKeyToken, culture));
            }

            return result;
        }
    }
}