using System;
using System.Data;

namespace DataPowerTools.DataReaderExtensibility.TransformingReaders
{
    /// <summary>
    /// Disposes underlying disposables upon DataReader disposal.
    /// </summary>
    /// <typeparam name="TDataReader"></typeparam>
    public class DisposingDataReader<TDataReader> : ExtensibleDataReader<TDataReader>
        where TDataReader : IDataReader
    {
        private readonly IDisposable[] _disposables;

        /// <summary>
        /// A data reader that disposes of the passed disposables when disposed.
        /// The underlying data reader is also disposed when .Dispose() is called.
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="disposables">The underlying disposables to dispose when the reader is disposed.</param>
        public DisposingDataReader(TDataReader dataReader, params IDisposable[] disposables) : base(dataReader)
        {
            _disposables = disposables;
        }

        /// <summary>
        /// Disposes underlying disposables including DataReader.
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