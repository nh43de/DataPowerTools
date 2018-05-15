using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="Task"/> class, and task-related functionality.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits for a handle to be signalled, allowing cancellation.
        /// </summary>
        /// <param name="waitHandle">The wait handle to observe.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="OperationCanceledException">The cancellation token was signalled before the operation completed.</exception>
        public static void WaitOne(this WaitHandle waitHandle, CancellationToken cancellationToken)
        {
            waitHandle.WaitOne(Timeout.Infinite, cancellationToken);
        }

        /// <summary>
        /// Waits for a handle to be signalled for a specified time, allowing cancellation. Returns <c>true</c> if the handle was signalled, and <c>false</c> if there was a timeout.
        /// </summary>
        /// <param name="waitHandle">The wait handle to observe.</param>
        /// <param name="millisecondsTimeout">The amount of time to wait for the handle to be signalled, in milliseconds; or <see cref="Timeout.Infinite"/> for an infinite wait.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="OperationCanceledException">The cancellation token was signalled before the operation completed.</exception>
        /// <returns>Returns <c>true</c> if the handle was signalled, and <c>false</c> if there was a timeout.</returns>
        public static bool WaitOne(this WaitHandle waitHandle, int millisecondsTimeout, CancellationToken cancellationToken)
        {
            var waitFor = new WaitHandle[] { waitHandle, cancellationToken.WaitHandle };
            int result = WaitHandle.WaitAny(waitFor, millisecondsTimeout);
            if (result == 1)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            return result != WaitHandle.WaitTimeout;
        }
    }
}
