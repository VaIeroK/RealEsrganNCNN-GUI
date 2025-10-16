using System;
using System.IO;
using IWshRuntimeLibrary;

namespace RealEsrgan_GUI
{
    static class PathResolver
    {
        public static string GetDirectoryPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (Directory.Exists(fullPath)) return fullPath;
            var resolved = ResolveShortcut(fullPath + ".lnk");
            if (!string.IsNullOrEmpty(resolved) && Directory.Exists(resolved)) return resolved;
            throw new DirectoryNotFoundException($"Directory '{path}' not found and no valid shortcut exists.");
        }

        public static string GetFilePath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (System.IO.File.Exists(fullPath)) return fullPath;
            var resolved = ResolveShortcut(fullPath + ".lnk");
            if (!string.IsNullOrEmpty(resolved) && System.IO.File.Exists(resolved)) return resolved;
            throw new FileNotFoundException($"File '{path}' not found and no valid shortcut exists.");
        }

        private static string ResolveShortcut(string lnkPath)
        {
            if (!System.IO.File.Exists(lnkPath)) return null;
            try
            {
                var shell = new WshShell();
                var shortcut = (IWshShortcut)shell.CreateShortcut(lnkPath);
                return shortcut.TargetPath;
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to read shortcut '{lnkPath}'.", ex);
            }
        }
    }
}