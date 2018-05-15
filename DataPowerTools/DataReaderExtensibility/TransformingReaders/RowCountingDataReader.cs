using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Outputs row count into Depth property.
    /// </summary>
    public class RowCountingDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly IProgress<int> _progress;
        private int _i = 0;

        public RowCountingDataReader(TDataReader dataReader, IProgress<int> progress = null) : base(dataReader)
        {
            _progress = progress;
        }

        public override bool Read()
        {
            var d = base.Read();

            if (d)
                _i++;

            _progress?.Report(_i);

            return d;
        }

        public override int Depth => _i;
    }
}