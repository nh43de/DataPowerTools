using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public interface IDiagnosticDataReader : IDataReader
    {
        string GetReaderDiagnosticInfo();
    }
}