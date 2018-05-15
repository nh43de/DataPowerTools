using System.IO;

namespace DataPowerTools.Extensions
{
    public static class FileExtensions
    {
        public static string ReadAllText(this FileInfo fileInfo)
        {
            return File.ReadAllText(fileInfo.FullName);
        }

        /// <summary>
        ///     Writes to file and returns a string
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="outputPath"></param>
        /// <returns></returns>
        public static string WriteToFile(this string txt, string outputPath)
        {
            File.WriteAllText(outputPath, txt);
            return txt;
        }
    }
}