using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public delegate bool RowInclusionCondition(IDataReader reader);
}