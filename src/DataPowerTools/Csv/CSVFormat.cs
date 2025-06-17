using System.Text;

namespace DataPowerTools
{
    /// <summary>
    /// Specifies the format for CSV output
    /// </summary>
    public enum CSVFormat
    {
        /// <summary>
        /// ANSI encoding (Windows-1252) - Default
        /// </summary>
        ANSI,
        
        /// <summary>
        /// UTF-8 encoding with BOM
        /// </summary>
        UTF8,
        
        /// <summary>
        /// UTF-16 encoding (Unicode)
        /// </summary>
        UTF16,
        
        /// <summary>
        /// UTF-16 Little Endian encoding
        /// </summary>
        UTF16LE,
        
        /// <summary>
        /// UTF-16 Big Endian encoding
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
                CSVFormat.ANSI => Encoding.GetEncoding(1252), // Windows-1252
                CSVFormat.UTF8 => new UTF8Encoding(true), // UTF-8 with BOM
                CSVFormat.UTF16 => Encoding.Unicode, // UTF-16 LE
                CSVFormat.UTF16LE => Encoding.Unicode, // UTF-16 LE
                CSVFormat.UTF16BE => Encoding.BigEndianUnicode, // UTF-16 BE
                _ => Encoding.GetEncoding(1252) // Default to ANSI
            };
        }
    }
} 