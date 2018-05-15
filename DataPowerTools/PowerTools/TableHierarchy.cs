namespace DataPowerTools.PowerTools
{
    /// <summary>
    /// Simple class that shows the table name and FK heirarchy of a table.
    /// </summary>
    public class TableHierarchy
    {
        /// <summary>
        /// FK graph level. The lower the level, the fewer dependencies it has. For deletion, order by RLevel 
        /// Ascending (0, 1, 2, ..) , for insertion, insert by RLevel descending (6, 5, 4, ..).
        /// </summary>
        public byte Level { get; set; }
        public string TableName { get; set; }
        public string SchemaName { get; set; }
        public bool IsSelfReferencing { get; set; }
        /// <summary>
        /// Two tables that reference each other.
        /// </summary>
        public bool HasExcludedRelationship { get; set; }
    }
}
