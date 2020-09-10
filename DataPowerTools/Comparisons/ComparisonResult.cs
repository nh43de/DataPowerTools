namespace DataPowerTools.Extensions
{
    public class ComparisonResult
    {
        public string FieldName { get; set; }
        public object SelfValue { get; set; }
        public object ToValue { get; set; }

        public bool IsMatch { get; set; }
    }
}