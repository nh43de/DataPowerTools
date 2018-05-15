using System;
using System.Collections.Generic;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// An enumerable that is disposable.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDisposableEnumerable<out T> : IDisposable, IEnumerable<T>
    {
        
    }
}