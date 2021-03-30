using System;
using System.Data;

namespace DataPowerTools
{
    /// <summary>
    /// A data reader that is disposable.
    /// </summary>
    public interface IDisposableDataReader : IDisposable, IDataReader
    {
        
    }
}