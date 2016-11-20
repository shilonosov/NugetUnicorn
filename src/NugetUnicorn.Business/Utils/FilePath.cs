using System.IO;

namespace NugetUnicorn.Business.Utils
{
    public class FilePath
    {
        public string DirectoryPath { get; }

        public string FileName { get; }

        public string FullPath { get; }

        public FilePath(string fullPath)
        {
            FullPath = fullPath;
            DirectoryPath = Path.GetDirectoryName(fullPath);
            FileName = Path.GetFileName(fullPath);
        }
    }
}