using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    public class UnionDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private bool _hasSwitched = false;

        private readonly TDataReader _dataReaderSecond;

        public UnionDataReader(TDataReader dataReaderFirst, TDataReader dataReaderSecond) : base(dataReaderFirst)
        {
            _dataReaderSecond = dataReaderSecond;
        }

        public override bool Read()
        {
            var underlyingRead = DataReader.Read();

            if (underlyingRead)
                return true;

            //if we reach the end of the underlying dr, switch to second
            if (_hasSwitched == false)
            {
                _hasSwitched = true;
                DataReader = _dataReaderSecond;
                return _dataReaderSecond.Read();
            }

            return false;
        }
    }
}