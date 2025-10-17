using System;
using System.IO;
using System.Linq;

namespace RealEsrgan_GUI
{
    internal static class DiskHelpers
    {
        private const long DefaultMinDiskBytes = 512L * 1024L * 1024L; // 512 MB
        private const long DiskSafetyMargin = 100L * 1024L * 1024L; // 100 MB

        public static long GetDriveAvailableFreeSpace(string anyPathOnDrive)
        {
            try
            {
                var full = Path.GetFullPath(string.IsNullOrEmpty(anyPathOnDrive) ? AppDomain.CurrentDomain.BaseDirectory : anyPathOnDrive);
                var root = Path.GetPathRoot(full);
                if (string.IsNullOrEmpty(root))
                    root = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
                var di = new DriveInfo(root);
                return di.AvailableFreeSpace;
            }
            catch
            {
                return 0;
            }
        }

        public static bool HasEnoughDiskSpace(long availableBytes, long requiredBytes)
        {
            if (availableBytes == 0) return false;
            return availableBytes >= (requiredBytes + DiskSafetyMargin);
        }

        public static bool HasEnoughDiskSpaceForPath(string anyPathOnDrive, long requiredBytes)
        {
            var available = GetDriveAvailableFreeSpace(anyPathOnDrive);
            return HasEnoughDiskSpace(available, requiredBytes);
        }

        public static long GetDirectorySizeBytes(string folder)
        {
            try
            {
                if (!Directory.Exists(folder)) return 0;
                long size = 0;
                foreach (var f in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    try { size += new FileInfo(f).Length; } catch { }
                }
                return size;
            }
            catch
            {
                return 0;
            }
        }

        // Optional helper to estimate required disk for unpacking/processing
        public static long EstimateRequiredForUnpack(long inputFileBytes)
        {
            // heuristic: frames can take ~2x input size; ensure minimum
            long est = Math.Max(inputFileBytes * 2, DefaultMinDiskBytes);
            return est;
        }

        public static long EstimateRequiredForProcessing(long inputFramesBytes)
        {
            // heuristic: need space for input frames + output frames (~2x) and margin
            long est = Math.Max(inputFramesBytes * 2, DefaultMinDiskBytes);
            return est;
        }
    }
}