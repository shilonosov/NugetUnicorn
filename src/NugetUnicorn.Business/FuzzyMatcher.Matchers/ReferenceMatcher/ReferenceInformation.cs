using System;
using System.IO;
using System.Reflection;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher
{
    public class ReferenceInformation
    {
        public string AssemblyName { get; private set; }
        public string Version { get; private set; }

        public ReferenceInformation(string fullPath)
        {
            if (!Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException($"expected full path, but relative received - {fullPath}");
            }

            var assembly = Assembly.LoadFile(fullPath);
            var assemblyName = assembly.GetName();
            AssemblyName = assemblyName.Name;
            Version = assemblyName.Version.ToString();
        }

        public ReferenceInformation(string assemblyName, string version)
        {
            AssemblyName = assemblyName;
            Version = version;
        }

        public override string ToString()
        {
            return $"{AssemblyName}, {Version}";
        }
    }
}