
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.DataMovement;

namespace ADLSSync.Tests
{
    static class CopyContainer
    {
        internal static void Copy(string credentialName, string sourceAccountName, string targetAccountName, string containerName, CopyMethod copyMethod, TextWriter log, int threadCount)
        {
            var accessToken = MyStorage.AzureStorageAccessToken(credentialName);
            var sourceBlobClient = MyStorage.AzureStorageBlobClient(sourceAccountName, accessToken);
            var targetBlobClient = MyStorage.AzureStorageBlobClient(targetAccountName, accessToken);
            var sourceContainer = sourceBlobClient.GetContainerReference(containerName);
            var targetContainer = targetBlobClient.GetContainerReference(containerName);
            var sourceUserDelegationKey = sourceContainer.ServiceClient.GetUserDelegationKey(TimeSpan.FromHours(2));
            var targetUserDelegationKey = targetContainer.ServiceClient.GetUserDelegationKey(TimeSpan.FromHours(2));

            var totalBlobs = 0L;
            var totalBytes = 0L;
            var totalElapsed = Stopwatch.StartNew();

            var blobQueue = new BlockingCollection<CloudBlockBlob>(boundedCapacity: 128);
            var workerThreads = CreateWorkerThreads(threadCount).ToArray();

            foreach (var t in workerThreads) t.Start();

            var blobs = sourceContainer.ListBlobs(useFlatBlobListing: true).OfType<CloudBlockBlob>();
            foreach (var sourceBlob in blobs)
            {
                if (0 == sourceBlob.Properties.Length) continue;
                blobQueue.Add(sourceBlob);
            }
            blobQueue.CompleteAdding();

            foreach (var t in workerThreads) t.Join();

            totalElapsed.Stop();
            log.BlobCopySummary(totalBlobs, totalBytes, threadCount, copyMethod, totalElapsed.Elapsed);

            IEnumerable<Thread> CreateWorkerThreads(int threadCount)
            {
                for(int i=0; i<threadCount; i++)
                {
                    var t = new Thread(ThreadMain);
                    t.IsBackground = true;
                    yield return t;
                }
            }

            void ThreadMain(object ignoreMe)
            {
                try
                {
                    foreach (var blob in blobQueue.GetConsumingEnumerable()) CopyOneBlob(blob);
                }
                catch(ThreadAbortException)
                {
                    //
                }
            }

            void CopyOneBlob(CloudBlockBlob sourceBlob)
            {
                try
                {
                    if (null == sourceBlob) throw new ArgumentNullException(nameof(sourceBlob));

                    var timer = Stopwatch.StartNew();

                    var targetBlob = targetContainer.GetBlockBlobReference(sourceBlob.Name);
                    var sourceSasBlob = sourceBlob.ToSasBlobReference(sourceUserDelegationKey, SharedAccessBlobPermissions.Read);
                    var targetSasBlob = targetBlob.ToSasBlobReference(targetUserDelegationKey, SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write);
                    
                    //var transferContext = new SingleTransferContext();
                    TransferManager.CopyAsync(sourceSasBlob, targetSasBlob, copyMethod, options: null, context: null)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult();

                    timer.Stop();

                    Interlocked.Increment(ref totalBlobs);
                    Interlocked.Add(ref totalBytes, sourceBlob.Properties.Length);
                    
                    log.BlobCopyDuration(sourceBlob, copyMethod, timer.Elapsed, threadCount);
                }
                catch (Exception err)
                {
                    log.BlobCopyError(sourceBlob, copyMethod, err);
                }
            }
        }
    }
}

