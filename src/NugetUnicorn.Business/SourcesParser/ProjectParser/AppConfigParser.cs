using System;
using System.Collections.Generic;
using System.Xml;
using NugetUnicorn.Business.SourcesParser.ProjectParser.Models;
using NugetUnicorn.Utils.Extensions;

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
            doc.Load(configFilePath);

            var manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("bindings", "urn:schemas-microsoft-com:asm.v1");

            var root = doc.DocumentElement;
            if (root == null)
            {
                throw new ApplicationException($"error reading packages configuration file {configFilePath} -- can't load!");
            }

            var nodes = root.SelectNodes("/configuration/runtime/bindings:assemblyBinding/bindings:dependentAssembly", manager);
            if (nodes == null)
            {
                throw new ApplicationException($"error reading application configuration file {configFilePath} -- no root element found!");
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