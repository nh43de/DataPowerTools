using DataPowerTools.DataReaderExtensibility.Columns;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class SmartDataReaderDiagnosticInfo
    {
        public ColumnMappingInfo Mappings { get; set; }
        public string[] TransformGroups { get; set; }
        public FieldValueInfo[] NonStringDestinationValues { get; set; }
        public int Depth { get; set; }
    }
}