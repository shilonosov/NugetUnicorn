using System;
using System.IO;
using System.Reflection;

namespace NugetUnicorn.Business.FuzzyMatcher.Matchers.ReferenceMatcher
{
    public class ReferenceInformation
    {
        public string FullPath { get; }

        public string AssemblyName { get; }

        public string Version { get; }

        public ReferenceInformation(string fullPath)
        {
            FullPath = fullPath;
            if (!Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException($"expected full path, but relative received - {fullPath}");
            }
            if (!File.Exists(fullPath))
            {
                throw new ArgumentException($"Reference file could not be found in specified location: {fullPath}", nameof(fullPath));
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