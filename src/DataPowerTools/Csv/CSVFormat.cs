using System;
using System.Text;

namespace DataPowerTools
{
    /// <summary>
    /// Specifies the format for CSV output with Excel compatibility considerations.
    /// 
    /// For optimal Excel compatibility and emoji support, use UTF8 (default).
    /// If Excel shows one column, use Data > From Text/CSV and select UTF-8 encoding.
    /// </summary>
    public enum CSVFormat
    {
        /// <summary>
        /// ANSI encoding (Windows-1252) - Falls back to UTF-8 if not available.
        /// Limited emoji support. Use UTF8 for better compatibility.
        /// </summary>
        ANSI,
        
        /// <summary>
        /// UTF-8 encoding with BOM - Default and recommended for Excel compatibility.
        /// Supports all emojis and international characters. Excel-friendly with proper import.
        /// </summary>
        UTF8,
        
        /// <summary>
        /// UTF-16 encoding (Unicode) - Full Unicode support but larger file size.
        /// May require specific Excel import settings.
        /// </summary>
        UTF16,
        
        /// <summary>
        /// UTF-16 Little Endian encoding - Full Unicode support but larger file size.
        /// </summary>
        UTF16LE,
        
        /// <summary>
        /// UTF-16 Big Endian encoding - Full Unicode support but larger file size.
        /// </summary>
        UTF16BE
    }

    /// <summary>
    /// Extension methods for CSVFormat
    /// </summary>
    public static class CSVFormatExtensions
    {
        /// <summary>
        /// Gets the encoding for the specified CSV format
        /// </summary>
        public static Encoding GetEncoding(this CSVFormat format)
        {
            return format switch
            {
                CSVFormat.ANSI => GetAnsiEncoding(),
                CSVFormat.UTF8 => new UTF8Encoding(true), // UTF-8 with BOM
                CSVFormat.UTF16 => Encoding.Unicode, // UTF-16 LE
                CSVFormat.UTF16LE => Encoding.Unicode, // UTF-16 LE
                CSVFormat.UTF16BE => Encoding.BigEndianUnicode, // UTF-16 BE
                _ => new UTF8Encoding(true) // Default to UTF-8 with BOM
            };
        }

        /// <summary>
        /// Gets ANSI encoding (Windows-1252) with fallback to UTF-8 for .NET Core/.NET 5+
        /// </summary>
        private static Encoding GetAnsiEncoding()
        {
            try
            {
                // Try to get Windows-1252 encoding
                return Encoding.GetEncoding(1252);
            }
            catch (NotSupportedException)
            {
                // In .NET Core/.NET 5+, legacy encodings may not be available
                // Fall back to UTF-8 which supports all the same characters and more
                return new UTF8Encoding(false); // UTF-8 without BOM for ANSI compatibility
            }
            catch (ArgumentException)
            {
                // Fallback for any other encoding issues
                return new UTF8Encoding(false);
            }
        }
    }
} 