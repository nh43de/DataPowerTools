using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Information about a column.
    /// </summary>
    public class ColumnDisplayInformation : BasicDataColumnInfo
    {
        /// <summary>
        /// The display name for a column (may be different from column name).
        /// </summary>
        public string DisplayName { get; set; }
    }
}