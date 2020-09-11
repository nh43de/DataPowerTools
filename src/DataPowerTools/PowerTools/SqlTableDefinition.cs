using System.Collections.Generic;

namespace DataPowerTools.PowerTools
{
    public class SqlTableDefinition
    {
        public string TableName { get; set; }

        public List<SqlColumnDefinition> ColumnDefinitions { get; set; }
            = new List<SqlColumnDefinition>();

        public List<string> PrimaryKeyColumnNames { get; set; }
            = new List<string>();
    }
}