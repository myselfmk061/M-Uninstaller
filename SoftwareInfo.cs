using System;

namespace MunInstaller
{
    public class SoftwareInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Publisher { get; set; }
        public string InstallDate { get; set; }
        public string InstallLocation { get; set; }
        public string UninstallString { get; set; }
        public long Size { get; set; }
        public string DisplayIcon { get; set; }
        public string RegistryKey { get; set; }

        public string GetFormattedSize()
        {
            if (Size == 0) return "Unknown";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = Size;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        public override string ToString()
        {
            return Name;
        }
    }
}