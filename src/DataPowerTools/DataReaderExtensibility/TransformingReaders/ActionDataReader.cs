using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Executes an action on every IDataReader read.
    /// </summary>
    public class ActionDataReader<TDataReader> : ExtensibleDataReader<TDataReader> where TDataReader : IDataReader
    {
        private readonly Action<TDataReader> _readerAction;
        
        public ActionDataReader(TDataReader dataReader, Action<TDataReader> readerAction) : base(dataReader)
        {
            _readerAction = readerAction;
        }

        public override bool Read()
        {
            _readerAction(DataReader);
            return DataReader.Read();
        }
    }
}