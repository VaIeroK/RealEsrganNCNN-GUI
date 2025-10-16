using System;
using System.IO;
using IWshRuntimeLibrary;

namespace RealEsrgan_GUI
{
    static class PathResolver
    {
        public static string GetDirectoryPath(string path)
        {
            string fullPath = Path.GetFullPath(path);

            if (Directory.Exists(fullPath))
            {
                return fullPath;
            }

            string lnkPath = fullPath + ".lnk";
            if (System.IO.File.Exists(lnkPath))
            {
                try
                {
                    var shell = new WshShell();
                    var shortcut = (IWshShortcut)shell.CreateShortcut(lnkPath);
                    string resolvedPath = shortcut.TargetPath;

                    if (Directory.Exists(resolvedPath))
                    {
                        return resolvedPath;
                    }

                    throw new DirectoryNotFoundException($"Shortcut '{lnkPath}' does not point to an existing directory.");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to read shortcut '{lnkPath}'.", ex);
                }
            }

            throw new DirectoryNotFoundException($"Directory '{path}' not found and no valid shortcut '{lnkPath}' exists.");
        }

        public static string GetFilePath(string path)
        {
            string fullPath = Path.GetFullPath(path);

            if (System.IO.File.Exists(fullPath))
            {
                return fullPath;
            }

            string lnkPath = fullPath + ".lnk";
            if (System.IO.File.Exists(lnkPath))
            {
                try
                {
                    var shell = new WshShell();
                    var shortcut = (IWshShortcut)shell.CreateShortcut(lnkPath);
                    string resolvedPath = shortcut.TargetPath;

                    if (System.IO.File.Exists(resolvedPath))
                    {
                        return resolvedPath;
                    }
                    throw new FileNotFoundException($"Shortcut '{lnkPath}' does not point to an existing file.");
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to read shortcut '{lnkPath}'.", ex);
                }
            }
            throw new FileNotFoundException($"File '{path}' not found and no valid shortcut '{lnkPath}' exists.");
        }
    }
}
