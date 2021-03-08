
using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;

namespace ADLSSync.Tests
{
    internal static class LogHelpers
    {
        static readonly object LogGate = new object();

        internal static TextWriter CreateOrOpenSyncLogFile(string logFileName)
        {
            var appendPsvHeader = !File.Exists(logFileName);
            var writer = File.AppendText(logFileName);
            if (appendPsvHeader) writer.WriteLine("Method|ThreadCount|BlobName|SizeMB|Mbps|MilliSeconds");
            return writer;
        }

        internal static void BlobCopyDuration(this TextWriter writer, CloudBlockBlob blobInfo, CopyMethod copyMethod, TimeSpan duration, int threadCount)
        {
            var blobName = blobInfo.Name;
            var bytes = blobInfo.Properties.Length;
            var mBytes = bytes / 1024 / 1024;
            var mBits = mBytes * 8;
            var mbps = (double)mBits / duration.TotalSeconds;

            lock (LogGate)
            {
                writer.WriteLine($"{copyMethod} | {threadCount} | {blobName} | {mBytes} | {mbps:#.00} | {duration.TotalMilliseconds:#}");
            }
        }

        internal static void BlobCopyError(this TextWriter writer, CloudBlockBlob blobInfo, CopyMethod copyMethod, Exception err)
        {
            var blobName = blobInfo.Name;

            var rootCause = string.Empty;
            while(null != err)
            {
                rootCause = err.Message;
                err = err.InnerException;
            }

            lock (LogGate)
            {
                writer.WriteLine($"{copyMethod}.ERROR | {blobName} | {rootCause}");
            }
        }

        internal static void BlobCopySummary(this TextWriter writer, long totalBlobs, long totalBytes, int threadCount, CopyMethod copyMethod, TimeSpan duration)
        {
            var mBytes = totalBytes / 1024 / 1024;
            var mBits = mBytes * 8;
            var mbps = (double)mBits / duration.TotalSeconds;

            lock (LogGate)
            {
                writer.WriteLine($"{copyMethod}-Total | {threadCount} thread | {totalBlobs} blobs | {mBytes} MB | {mbps:#.00} Mbps | {duration.TotalMilliseconds:#} ms");
            }
        }
    }
}
