using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Information about a column.
    /// </summary>
    public class CsharpTypeColumnInformation : BasicDataColumnInfo
    {
        /// <summary>
        /// The display name for a column as given in the schema annotations (e.g. Column("id_name")), which may be different from the field name.
        /// </summary>
        public string DisplayName { get; set; }

        public bool IsNonStringReferenceType { get; set; }
    }
}