using System;

namespace DataPowerTools.Helpers;

public static class FileNameHelpers
{
    //TODO: This needs to be an enum passed in ie. string GetDateFileName( ..., FileDateFormatType fileDateFormatType ). 
    //TODO: also extension options? (eg. xlsx  vs  no extension  vs  .xlsx)
    //TODO: rename class
    [Obsolete]
    public static string GetDateTimeFileName(string prefix, string ext, DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{prefix} {d.Year}-{d.Month}-{d.Day}_{d.Hour}-{d.Minute}-{d.Second}.{ext}";
    }

    public static string GetDateFileName(string prefix, string ext, DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{prefix} {d.Year}-{d.Month}-{d.Day}.{ext}";
    }

    public static string GetDateFileNameString(DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{d.Year}-{d.Month}-{d.Day}";
    }

    public static string GetTimeFileName(string prefix, string ext, DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{prefix} {d.Hour}-{d.Minute}-{d.Second}.{ext}";
    }

    public static string GetDateTimeFileName(DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{d.Year}-{d.Month}-{d.Day}_{d.Hour}-{d.Minute}-{d.Second}";
    }

    public static string GetDateFileName(DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{d.Year}-{d.Month}-{d.Day}";
    }

    public static string GetTimeFileName(DateTime? date = null)
    {
        var d = date ?? DateTime.Now;
        return $"{d.Hour}-{d.Minute}-{d.Second}";
    }
}