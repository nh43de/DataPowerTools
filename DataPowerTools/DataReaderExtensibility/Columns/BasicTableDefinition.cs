using System.Collections.Generic;

namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class BasicTableDefinition
    {
        public string TableName { get; set; }

        public IEnumerable<BasicDataFieldInfo> Fields { get; set; }
    }
}