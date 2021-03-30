using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Disposes underlying disposables upon datareader disposal.
    /// </summary>
    /// <typeparam name="TDataReader"></typeparam>
    public class DisposingDataReader<TDataReader> : ExtensibleDataReader<TDataReader>, IDisposableDataReader
        where TDataReader : IDataReader
    {
        private readonly IDisposable[] _disposables;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="disposables">The underlying disposables to dispose when the reader is disposed.</param>
        public DisposingDataReader(TDataReader dataReader, params IDisposable[] disposables) : base(dataReader)
        {
            _disposables = disposables;
        }

        /// <summary>
        /// Disposes underlyinh
        /// </summary>
        public override void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            DataReader.Dispose();
        }
    }
}