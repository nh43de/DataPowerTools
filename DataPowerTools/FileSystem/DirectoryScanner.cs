using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools.FileSystem
{
    /// <summary>
    /// Provides classes for scanning directories and performing actions based on files found. Can be optimized to be made parallel. (Add scanparallel methods).
    /// </summary>
    public static class DirectoryScanner
    {
        /// <summary>
        /// Scans a directory recursive in parallel.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="directoryFunc">If function returns false for a directory then do not search this director.</param>
        /// <param name="finishDirectoryFunc">When a directory is finished being searched.</param>
        /// <param name="fileAction"></param>
        /// <param name="maxDop"></param>
        /// <param name="token"></param>
        public static void ScanRecursiveParallel(string rootDir, Func<DirectoryInfo, bool> directoryFunc, Action<DirectoryInfo> finishDirectoryFunc, Action<FileInfo> fileAction, int maxDop = 10, CancellationToken token = default(CancellationToken))
        {
            //recurse dirs too
            var dirs = new DirectoryInfo(rootDir).EnumerateDirectories();

            Parallel.ForEach(dirs, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDop,
                CancellationToken = token
            }, d => {
                if (directoryFunc(d))
                {
                    ScanRecursiveParallel(d.FullName, directoryFunc, finishDirectoryFunc, fileAction, maxDop, token);
                    finishDirectoryFunc(d);
                }
            });

            ScanStandardParallel(rootDir, fileAction, maxDop, token);
        }

        /// <summary>
        /// Scans a directory recursive in parallel.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="directoryFunc">If function returns false for a directory then do not search this director.</param>
        /// <param name="fileAction"></param>
        /// <param name="maxDop"></param>
        /// <param name="token"></param>
        public static void ScanRecursiveParallel(string rootDir, Func<DirectoryInfo, bool> directoryFunc, Action<FileInfo> fileAction, int maxDop = 10, CancellationToken token = default(CancellationToken))
        {
            //recurse dirs too
            var dirs = new DirectoryInfo(rootDir).EnumerateDirectories();

            Parallel.ForEach(dirs, new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDop,
                CancellationToken = token
            }, d => {
                if (directoryFunc(d))
                    ScanRecursiveParallel(d.FullName, directoryFunc, fileAction, maxDop, token);
            });

            ScanStandardParallel(rootDir, fileAction, maxDop, token);
        }


        /// <summary>
        /// Scans a directory recursive in parallel.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="fileAction"></param>
        /// <param name="maxDop"></param>
        public static void ScanRecursiveParallel(string rootDir, Action<FileInfo> fileAction, int maxDop = 10, CancellationToken token = default(CancellationToken))
        {
            //recurse dirs too
            var dirs = Directory.GetDirectories(rootDir);

            if (dirs.Length > maxDop)
            {
                Parallel.ForEach(dirs, new ParallelOptions {
                    MaxDegreeOfParallelism = maxDop,
                    CancellationToken = token
                }, d => {
                    ScanRecursiveParallel(d, fileAction, maxDop, token);
                });
            }
            else
            {
                foreach (var d in dirs)
                    ScanRecursive(d, fileAction);
            }

            ScanStandardParallel(rootDir, fileAction, maxDop, token);
        }

        /// <summary>
        /// Scans a directory non-recursively in parallel.
        /// </summary>
        /// <param name="rootDir"></param>
        /// <param name="fileAction"></param>
        /// <param name="maxDop"></param>
        public static void ScanStandardParallel(string rootDir, Action<FileInfo> fileAction, int maxDop = 10, CancellationToken token = default(CancellationToken))
        {
            var files = new DirectoryInfo(rootDir).GetFiles();

            if (files.Length > maxDop)
            {
                Parallel.ForEach(files, new ParallelOptions {
                    MaxDegreeOfParallelism = maxDop,
                    CancellationToken = token
                }, file => {
                    fileAction(file);
                });
            }
            else
            {
                foreach (var file in files)
                    fileAction(file);
            }
        }

        public static void ScanRecursive(string rootDir, Action<FileInfo> fileAction)
        {
            //recurse dirs too
            var dirs = Directory.GetDirectories(rootDir);
            foreach (var d in dirs)
                ScanRecursive(d, fileAction);

            ScanStandard(rootDir, fileAction);
        }

        public static void ScanStandard(string rootDir, Action<FileInfo> fileAction)
        {
            var files = new DirectoryInfo(rootDir).GetFiles();

            foreach (var file in files)
                fileAction(file);
        }

        public static void ScanRecursive(string rootDir, Action<string> fileAction)
        {
            //recurse dirs too
            var dirs = Directory.GetDirectories(rootDir);
            foreach (var d in dirs)
                ScanRecursive(d, fileAction);

            ScanStandard(rootDir, fileAction);
        }

        public static void ScanStandard(string rootDir, Action<string> fileAction)
        {
            var files = Directory.GetFiles(rootDir);

            foreach (var file in files)
                fileAction(file);
        }
    }
}