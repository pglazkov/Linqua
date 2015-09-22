using System;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.Persistence;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Linqua.Logging
{
	[Export(typeof(ILogSharingService))]
	internal class LogSharingService : ILogSharingService
	{
		private readonly IBackendServiceClient serviceClient;

		[ImportingConstructor]
		public LogSharingService([NotNull] IBackendServiceClient serviceClient)
		{
			Guard.NotNull(serviceClient, nameof(serviceClient));

			this.serviceClient = serviceClient;
		}

		public async Task<Uri> ShareCurrentLogAsync()
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
			using (var inputStream = await file.OpenReadAsync())
			{
				CloudBlockBlob blobFromSasCredential = container.GetBlockBlobReference(uploadInfo.ResourceName);
				await blobFromSasCredential.UploadFromStreamAsync(inputStream);
			}

			return uploadUri;
		}
	}
}