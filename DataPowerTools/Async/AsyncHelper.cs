using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools.Async
{
    /// <summary>
    /// Helper methods for async methods.
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        /// Runs the task synchronously.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs the task synchronously.
        /// </summary>
        /// <param name="func"></param>
        public static void RunSync(Func<Task> func)
        {
            _myTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
        
        /// <summary>
        /// Runs the task synchronously.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(this Task<TResult> task)
        {
            return RunSync(() => task);
        }

        /// <summary>
        /// Runs the task synchronously.
        /// </summary>
        /// <param name="task"></param>
        public static void RunSync(this Task task)
        {
            RunSync(() => task);
        }
    }
}