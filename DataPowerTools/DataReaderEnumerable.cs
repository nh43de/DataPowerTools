using System.Data;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Wraps a data reader as a disposable enumerable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataReaderEnumerable<T> : DisposableEnumerable<IDataReader, T> where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="useStrict"></param>
        public DataReaderEnumerable(IDataReader reader, bool useStrict = false) 
            : base(reader, dr => useStrict ? dr.Select<T>() : dr.SelectNonStrict<T>())
        {
        }
    }
}