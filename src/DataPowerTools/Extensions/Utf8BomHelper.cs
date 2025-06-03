using System;
using System.IO;

namespace DataPowerTools.Extensions;

public static class Utf8BomHelper
{
    private static readonly byte[] Utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };

    public static void EnsureUtf8Bom(string filePath)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);

        if (HasUtf8Bom(fileBytes))
        {
            return;
        }

        using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        fs.Write(Utf8Bom, 0, Utf8Bom.Length); // Add BOM
        fs.Write(fileBytes, 0, fileBytes.Length); // Write original content
        Console.WriteLine("BOM added.");
    }

    private static bool HasUtf8Bom(byte[] bytes)
    {
        return bytes.Length >= 3 &&
               bytes[0] == Utf8Bom[0] &&
               bytes[1] == Utf8Bom[1] &&
               bytes[2] == Utf8Bom[2];
    }
}