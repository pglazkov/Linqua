using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Framework;
using JetBrains.Annotations;
using Linqua.Persistence;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Linqua.Logging
{
    public sealed class LogSharingService : ILogSharingService
    {
        private readonly IBackendServiceClient serviceClient;

        public LogSharingService([NotNull] IBackendServiceClient serviceClient)
        {
            Guard.NotNull(serviceClient, nameof(serviceClient));

            this.serviceClient = serviceClient;
        }

        public IAsyncOperation<Uri> ShareCurrentLogAsync()
        {
            return ShareCurrentLogImplAsync().AsAsyncOperation();
        }

        private async Task<Uri> ShareCurrentLogImplAsync()
        {
            var uploadInfo = await serviceClient.GetLogUploadInfoAsync();

            // Get the URI generated that contains the SAS and extract the storage credentials.
            StorageCredentials cred = new StorageCredentials(uploadInfo.SasQueryString);
            var uploadUri = new Uri(uploadInfo.UploadUri);

            // Instantiate a Blob store container based on the info in the returned item.
            CloudBlobContainer container = new CloudBlobContainer(new Uri($"https://{uploadUri.Host}/{uploadInfo.ContainerName}"), cred);

            // Prepare the compressed log files for upload.
            var file = await FileStreamingTarget.Instance.GetCompressedLogFile();

            // Upload the ZIP to the blob storage

            CloudBlockBlob blobFromSasCredential = container.GetBlockBlobReference(uploadInfo.ResourceName);
            await blobFromSasCredential.UploadFromFileAsync(file);

            return uploadUri;
        }
    }
}