using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools
{
    /// <summary>
    ///     Tries to get things by repeating the action until successful, or number of tries has been met.
    /// </summary>
    public static class Try
    {
        /// <summary>
        ///     Tries to do an action. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful after number of retries, will throw an aggregate exception.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="tryCount"></param>
        public static void Do(
            this Action action,
            TimeSpan? retryInterval = null,
            int tryCount = 1)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < tryCount; retry++)
            {
                if (retry > 0 && retryInterval.HasValue)
                    Thread.Sleep(retryInterval.Value);

                try
                {
                    action();
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            throw new AggregateException(exceptions);
        }

        /// <summary>
        ///     Tries to do an action. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful after number of retries, will throw an aggregate exception.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="retryCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task DoAsync(
            this Func<CancellationToken, Task> action,
            CancellationToken token = default(CancellationToken),
            TimeSpan? retryInterval = null,
            int? retryCount = 1
        )
        {
            var exceptions = new List<Exception>();

            var numTries = 0;
            while (retryCount == null || numTries < retryCount)
            {
                if (numTries > 0 && retryInterval.HasValue)
                    await Task.Delay(retryInterval.Value, token);

                try
                {
                    await action(token);
                    return;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                numTries++;
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            throw new AggregateException(exceptions);
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            Func<Exception, T> replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            T replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement;
            }
        }

        /// <summary>
        ///     Tries to do evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful after number of retries, will throw an aggregate exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            TimeSpan? retryInterval = null,
            int retryCount = 1)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                if (retry > 0 && retryInterval.HasValue)
                    Thread.Sleep(retryInterval.Value);

                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            throw new AggregateException(exceptions);
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<CancellationToken, Task<T>> action,
            Func<Exception, T> replacement,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                return await action(token);
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<CancellationToken, Task<T>> action,
            T replacement,
            CancellationToken token = default(CancellationToken))
        {
            try
            {
                return await action(token);
            }
            catch (Exception ex)
            {
                return replacement;
            }
        }

        /// <summary>
        ///     Tries to do evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If retry count is set to null, will loop indefinitely. If not successful after number of retries, will throw an
        ///     aggregate exception.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="retryInterval"></param>
        /// <param name="retryCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(
            this Func<CancellationToken, Task<T>> action,
            CancellationToken token = default(CancellationToken),
            TimeSpan? retryInterval = null,
            int? retryCount = 1
        )
        {
            var exceptions = new List<Exception>();

            var numTries = 0;
            while (retryCount == null || numTries < retryCount)
            {
                if (numTries > 0 && retryInterval.HasValue)
                    await Task.Delay(retryInterval.Value, token);

                try
                {
                    return await action(token);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }

                numTries++;
            }

            if (exceptions.Count == 1)
                throw exceptions[0];

            throw new AggregateException(exceptions);
        }
    }
}