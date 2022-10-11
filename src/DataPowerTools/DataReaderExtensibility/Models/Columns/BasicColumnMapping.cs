namespace DataPowerTools.DataReaderExtensibility.Columns
{
    public class BasicColumnMapping
    {
        public BasicDataColumnInfo SourceField { get; set; }

        public TypedDataColumnInfo? DestinationField { get; set; }

        public override string ToString() => $"{SourceField} -> {DestinationField}";
    }
}