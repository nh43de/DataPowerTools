using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class RowLimitingDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly int _maxRows;

        private int _i = 0;

        public RowLimitingDataReader(TDataReader dataReader, int maxRows) : base(dataReader)
        {
            _maxRows = maxRows;
        }

        public override bool Read()
        {
            if (_i >= _maxRows)
                return false;

            _i++;

            return base.Read();
        }
    }
}