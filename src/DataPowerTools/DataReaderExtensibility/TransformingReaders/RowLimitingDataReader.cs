using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class RowLimitingDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly int _maxRows;

        public RowLimitingDataReader(TDataReader dataReader, int maxRows) : base(dataReader)
        {
            _maxRows = maxRows;
        }

        public override bool Read()
        {
            return Depth < _maxRows && base.Read();
        }
    }
}