using System;
using System.Collections;
using System.Collections.Generic;
using DataPowerTools.Extensions;

namespace DataPowerTools
{
    /// <summary>
    /// Wrapper class for the case where the underlying of an enumerable is disposable.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TDisposable"></typeparam>
    public class DisposableEnumerable<TDisposable, TItem> : IDisposableEnumerable<TItem> 
        where TItem : class
        where TDisposable : IDisposable
    {
        private readonly TDisposable _disposable;
        private readonly IEnumerable<TItem> _disposableEnumerable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="enumerate"></param>
        public DisposableEnumerable(TDisposable disposable, Func<TDisposable, IEnumerable<TItem>> enumerate)
        {
            _disposable = disposable;
            _disposableEnumerable = enumerate(disposable);
        }

        /// <summary>
        /// Dispose of the underlying.
        /// </summary>
        public void Dispose()
        {
            _disposable.Dispose();
        }

        /// <summary>
        /// Get enumerator of the underlying.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<TItem> GetEnumerator() => _disposableEnumerable.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => _disposableEnumerable.GetEnumerator();
    }
}