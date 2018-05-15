using System;
using System.Diagnostics;
using System.IO;

namespace DataPowerTools.FileSystem
{
    public static class Files
    {
        /// <summary>
        /// Makes a file readonly.
        /// </summary>
        /// <param name="path"></param>
        public static void MakeReadonly(string path)
        {
            File.SetAttributes(path, FileAttributes.ReadOnly);
        }

        /// <summary>
        /// Starts a new process and opens a file based on the default shell file handler.
        /// </summary>
        /// <param name="path"></param>
        public static void OpenDocument(string path)
        {
            Process.Start(path);
        }



        //TODO: This needs to be an enum passed in ie. string GetDateFileName( ..., FileDateFormatType fileDateFormatType ). 
        //TODO: also extension options? (eg. xlsx  vs  no extension  vs  .xlsx)
        //TODO: rename class
        [Obsolete]
        public static string GetDateTimeFileName(string prefix, string ext, DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{prefix} {d.Year}-{d.Month}-{d.Day}_{d.Hour}-{d.Minute}-{d.Second}.{ext}";
        }
        [Obsolete]
        public static string GetDateFileName(string prefix, string ext, DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{prefix} {d.Year}-{d.Month}-{d.Day}.{ext}";
        }
        [Obsolete]
        public static string GetDateFileNameString(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{d.Year}-{d.Month}-{d.Day}";
        }
        [Obsolete]
        public static string GetTimeFileName(string prefix, string ext, DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{prefix} {d.Hour}-{d.Minute}-{d.Second}.{ext}";
        }
        [Obsolete]
        public static string GetDateTimeFileName(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{d.Year}-{d.Month}-{d.Day}_{d.Hour}-{d.Minute}-{d.Second}";
        }
        [Obsolete]
        public static string GetDateFileName(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{d.Year}-{d.Month}-{d.Day}";
        }
        [Obsolete]
        public static string GetTimeFileName(DateTime? date = null)
        {
            var d = date ?? DateTime.Now;
            return $"{d.Hour}-{d.Minute}-{d.Second}";
        }
    }
}