using System;
using System.Collections.Generic;

namespace NugetUnicorn.Business.SourcesParser.ProjectParser.Structure
{
    public class TargetFramework : ProjectStructureItem
    {
        public static List<string> FolderNames = new List<string>()
            {
                "net35",
                "net40",
                "net45",
                "net451",
                "net452",
                "net46",
                "net461",
                "net462"
            };

        public static string GetLowerVersionFolder(string shortFolderName)
        {
            var thisIndex = FolderNames.IndexOf(shortFolderName);
            if (thisIndex == -1)
            {
                throw new ArgumentException($"framework with short folder name [{shortFolderName}] is not supported at this moment. please issue a bug at https://github.com/shilonosov/NugetUnicorn/issues", nameof(shortFolderName));
            }

            if (thisIndex == 0)
            {
                throw new ApplicationException($"there is no lower version folder name for [{shortFolderName}]");
            }

            return FolderNames[thisIndex - 1];
        }

        public string ShortFolderName { get; }

        public string Version { get; }

        public TargetFramework(string targetFrameworkVersion)
        {
            Version = targetFrameworkVersion;
            ShortFolderName = GetShortFolderName(targetFrameworkVersion);
        }

        //TODO: [DS] the one may add missing from this page: https://docs.microsoft.com/en-us/nuget/schema/target-frameworks#supported-frameworks
        private string GetShortFolderName(string targetFrameworkVersion)
        {
            switch (targetFrameworkVersion)
            {
                case "v3.5": return "net35";
                case "v4.0": return "net40";
                case "v4.5": return "net45";
                case "v4.5.1": return "net451";
                case "v4.5.2": return "net452";
                case "v4.6": return "net46";
                case "v4.6.1": return "net461";
                case "v4.6.2": return "net462";
                default:
                    throw new ApplicationException(
                        $"unfortunatelly, this .net version [{targetFrameworkVersion}] is not supported at this moment. please issue a bug at https://github.com/shilonosov/NugetUnicorn/issues");
            }
        }
    }
}