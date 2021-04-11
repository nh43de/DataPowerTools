using System;
using System.Data;
using DataPowerTools.Extensions;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Outputs row count into Depth property.
    /// </summary>
    public class NotifyingDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly Action<int> _progress;
        private readonly int? _modulus;
        private int _i = 0;
        
        public NotifyingDataReader(TDataReader dataReader, Action<int> progress = null, int? modulus = 1) : base(dataReader)
        {
            _progress = progress;
            _modulus = modulus;
        }

        public override bool Read()
        {
            var d = base.Read();
            
            if (d)
            {
                _i++;

                if (_modulus.HasValue)
                {
                    if (_i % _modulus == 0)
                        Report(_i);
                }
                else
                    Report(_i);
            }
            else
            {
                if (_modulus.HasValue)
                {
                    if (_i % _modulus != 0)
                        Report(_i);
                }
            }
            
            return d;
        }

        private void Report(int i)
        {
            _progress?.Invoke(i);
        }
    }
}