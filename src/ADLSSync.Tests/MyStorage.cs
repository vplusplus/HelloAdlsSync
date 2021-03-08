
using System;
using System.Threading;
using Azure.Core;
using Azure.Identity.Extensions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;

namespace ADLSSync.Tests
{
    internal static class MyStorage
    {

        internal static AccessToken AzureStorageAccessToken(string credentialName)
        {
            if (null == credentialName) throw new ArgumentNullException(nameof(credentialName));

            var tokenRequestContext = new TokenRequestContext(
                scopes: new[] { "https://storage.azure.com/.default" }
            );

            return My.Services
                .GetService<IConfidentialClientCredentialProvider>()
                .GetTokenCredential(credentialName)
                .GetToken(tokenRequestContext, CancellationToken.None)
                ;
        }

        internal static CloudBlobClient AzureStorageBlobClient(string storageAccountName, AccessToken accessToken)
        {
            if (null == storageAccountName) throw new ArgumentNullException(nameof(storageAccountName));
            if (null == accessToken.Token) throw new ArgumentNullException(nameof(accessToken));

            var tokenCredential = new Microsoft.Azure.Storage.Auth.TokenCredential(accessToken.Token);
            var storageCredential = new StorageCredentials(tokenCredential);
            var blobStorageUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");

            return new CloudBlobClient(blobStorageUri, storageCredential);

        }

        internal static UserDelegationKey GetUserDelegationKey(this CloudBlobClient blobClient, TimeSpan duration)
        {
            if (null == blobClient) throw new ArgumentNullException(nameof(blobClient));

            var udkStart = DateTimeOffset.Now.AddMinutes(-1);
            var udkEnd = udkStart.Add(duration);

            return blobClient.GetUserDelegationKey(udkStart, udkEnd);
        }

        internal static CloudBlockBlob ToSasBlobReference(this CloudBlockBlob blobReference, UserDelegationKey userDelegationKey, SharedAccessBlobPermissions permissions)
        {
            if (null == blobReference) throw new ArgumentNullException(nameof(blobReference));
            if (null == userDelegationKey) throw new ArgumentNullException(nameof(userDelegationKey));

            var policy = new SharedAccessBlobPolicy()
            {
                Permissions = permissions,
                SharedAccessStartTime = userDelegationKey.SignedStart.Value.AddMinutes(1),
                SharedAccessExpiryTime = userDelegationKey.SignedExpiry.Value.AddMinutes(-1)
            };

            var sasToken = blobReference.GetUserDelegationSharedAccessSignature(userDelegationKey, policy); //.TrimStart('?');
            var sasUri = new UriBuilder(blobReference.Uri) { Query = sasToken };
            var sasBlobReference = new CloudBlockBlob(sasUri.Uri);

            return sasBlobReference;
        }
    }
}
