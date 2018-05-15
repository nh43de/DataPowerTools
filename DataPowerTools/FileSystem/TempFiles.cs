using System;
using System.IO;

namespace DataPowerTools.FileSystem
{
    /// <summary>
    /// Methods pertaining to handling of temporary files.
    /// </summary>
    public static class TempFiles
    {
        /// <summary>
        /// Creates a temporary folder, invokes a function that takes in a directory path, creates the file and returns the
        /// full file path, then makes the file readonly and/or opens it.
        /// </summary>
        /// <param name="createFileAction">
        /// An action with one input parameter - the temp folder - that creates the file; and one
        /// output - the full path of the created file.
        /// </param>
        /// <param name="makeReadonly"></param>
        /// <param name="open"></param>
        /// <returns></returns>
        public static void CreateTempAndOpen(Func<string, string> createFileAction, bool makeReadonly = false,
            bool open = true)
        {
            var folder = CreateTempFolder();

            var file = createFileAction?.Invoke(folder);

            if (makeReadonly)
                Files.MakeReadonly(file);

            if (open)
                Files.OpenDocument(file);
        }

        /// <summary>
        /// Creates a temporary folder in the user environment temp path and returns the folder path.
        /// </summary>
        /// <returns></returns>
        public static string CreateTempFolder()
        {
            var tmpPath = Path.GetTempPath();

            var tmpFolder = Path.Combine(tmpPath, Guid.NewGuid().ToString().Substring(24));

            while (Directory.Exists(tmpFolder))
                tmpFolder = Path.Combine(tmpPath, Guid.NewGuid().ToString().Substring(24));

            Directory.CreateDirectory(tmpFolder);

            return tmpFolder;
        }
    }
}