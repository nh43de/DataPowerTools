using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Provides methods useful when dealing with streams.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Attempts to get the length of the stream. Returns -1 if the length of the stream could not be determined.
        /// </summary>
        /// <param name="stream">The stream to query.</param>
        /// <returns>The length of the stream, or -1 if the stream has an undefined length.</returns>
        public static long TryGetLength(this Stream stream)
        {
            try
            {
                return stream.Length;
            }
            catch (NotSupportedException)
            {
                return -1;
            }
        }

        /// <summary>
        /// Synchronously copies the contents of this stream into another stream, enabling cancellation.
        /// </summary>
        /// <param name="source">The stream that is the source of the copy.</param>
        /// <param name="destination">The stream that is the destination of the copy.</param>
        /// <param name="buffer">The buffer used by the copy. The size of this buffer determines the sizes of reads and writes made to the streams.</param>
        /// <param name="progress">A callback method invoked with the number of bytes transferred so far. May be null.</param>
        /// <param name="cancellationToken">A cancellation token which may be used to cancel the stream copy. May be null.</param>
        public static void CopyTo(this Stream source, Stream destination, byte[] buffer, IProgress<long> progress, CancellationToken? cancellationToken = null)
        {
            try
            {
                long bytesTransferred = 0;
                while (true)
                {
                    var bytesRead = source.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    destination.Write(buffer, 0, bytesRead);
                    if (progress != null)
                    {
                        bytesTransferred += bytesRead;
                        progress.Report(bytesTransferred);
                    }

                    cancellationToken?.ThrowIfCancellationRequested();
                }
            }
            catch
            {
                cancellationToken?.ThrowIfCancellationRequested();

                throw;
            }
        }

        /// <summary>
        /// Synchronously reads the contents of this stream as a sequence of byte buffers, enabling cancellation.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="buffer">The buffer used by the copy. The size of this buffer determines the sizes of reads made to the source stream.</param>
        /// <param name="cancellationToken">A cancellation token which may be used to cancel the stream copy.</param>
        public static IEnumerable<byte[]> GetConsumingEnumerable(this Stream source, byte[] buffer, CancellationToken cancellationToken)
        {
            while (true)
            {
                var bytesRead = source.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }

                yield return buffer;

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Synchronously reads the contents of this stream as a sequence of byte buffers.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="buffer">The buffer used by the copy. The size of this buffer determines the sizes of reads made to the source stream.</param>
        public static IEnumerable<byte[]> GetConsumingEnumerable(this Stream source, byte[] buffer)
        {
            return source.GetConsumingEnumerable(buffer, CancellationToken.None);
        }
    }
}
