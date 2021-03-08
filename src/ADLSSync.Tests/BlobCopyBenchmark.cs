
using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Azure.Storage.DataMovement;

namespace ADLSSync.Tests
{
    [TestClass]
    public class BlobCopyBenchmark
    {
        const string CredentialName = "aad";
        const string SourceStorageAccountName = "adlssyncsource210214";
        const string TargetStorageAccountName = "adlssynctarget210214";
        const string SampleContainerName = "data001";
        const string LogFileName = "D:/Junk/Logs/ADLSSyncLog.log";

        const int THREAD_COUNT = 32;

        [TestMethod]
        public void SyncCopy()
        {
            TransferManager.Configurations.ParallelOperations = Environment.ProcessorCount * 8;
            ServicePointManager.DefaultConnectionLimit = TransferManager.Configurations.ParallelOperations * 2;

            const CopyMethod copyMethod = CopyMethod.SyncCopy;

            using (var writer = LogHelpers.CreateOrOpenSyncLogFile(LogFileName))
            {
                CopyContainer.Copy(CredentialName, SourceStorageAccountName, TargetStorageAccountName, SampleContainerName, copyMethod, writer, THREAD_COUNT);
            }
        }

        [TestMethod]
        public void ServiceSideSyncCopy()
        {
            TransferManager.Configurations.ParallelOperations = Environment.ProcessorCount * 8;
            ServicePointManager.DefaultConnectionLimit = TransferManager.Configurations.ParallelOperations * 2;

            const CopyMethod copyMethod = CopyMethod.ServiceSideSyncCopy;

            using (var writer = LogHelpers.CreateOrOpenSyncLogFile(LogFileName))
            {
                CopyContainer.Copy(CredentialName, SourceStorageAccountName, TargetStorageAccountName, SampleContainerName, copyMethod, writer, THREAD_COUNT);
            }
        }

        [TestMethod]
        public void ServiceSideAsyncCopy()
        {
            TransferManager.Configurations.ParallelOperations = Environment.ProcessorCount * 8;
            ServicePointManager.DefaultConnectionLimit = TransferManager.Configurations.ParallelOperations * 2;

            const CopyMethod copyMethod = CopyMethod.ServiceSideAsyncCopy;

            using (var writer = LogHelpers.CreateOrOpenSyncLogFile(LogFileName))
            {
                CopyContainer.Copy(CredentialName, SourceStorageAccountName, TargetStorageAccountName, SampleContainerName, copyMethod, writer, THREAD_COUNT);
            }
        }
    }
}
