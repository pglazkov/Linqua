using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using Linqua.Service.DataObjects;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Linqua.Service.Controllers
{
    [AuthorizeLevel(AuthorizationLevel.User)]
    public class LogUploadInfoController : ApiController
    {
        // GET api/RandomEntry
        public async Task<LogUploadInfo> Get()
        {
            string storageAccountName = ConfigurationManager.AppSettings["STORAGE_ACCOUNT_NAME"];
            string storageAccountKey = ConfigurationManager.AppSettings["STORAGE_ACCOUNT_ACCESS_KEY"];

            // Try to get the Azure storage account token from app settings.  
            if (string.IsNullOrEmpty(storageAccountName) || string.IsNullOrEmpty(storageAccountKey))
            {
                throw new InvalidOperationException("Could not retrieve storage account settings.");
            }

            var currentUser = (ServiceUser)User;

            var result = new LogUploadInfo();

            // Set the URI for the Blob Storage service.
            Uri blobEndpoint = new Uri($"https://{storageAccountName}.blob.core.windows.net");

            // Create the BLOB service client.
            CloudBlobClient blobClient = new CloudBlobClient(blobEndpoint, new StorageCredentials(storageAccountName, storageAccountKey));

            const string ContainerName = "logfiles";

            result.ContainerName = ContainerName;

            // Create a container, if it doesn't already exist.
            CloudBlobContainer container = blobClient.GetContainerReference(ContainerName);
            await container.CreateIfNotExistsAsync();

            // Create a shared access permission policy. 
            BlobContainerPermissions containerPermissions = new BlobContainerPermissions
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            };

            // Enable anonymous read access to BLOBs.
            container.SetPermissions(containerPermissions);

            // Define a policy that gives write access to the container for 5 minutes.                                   
            SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(5),
                Permissions = SharedAccessBlobPermissions.Write
            };

            // Get the SAS as a string.
            result.SasQueryString = container.GetSharedAccessSignature(sasPolicy);

            // Set the URL used to store the image.
            result.ResourceName = DateTime.UtcNow.ToString("O") + currentUser.Id + ".zip";
            result.UploadUri = $"{blobEndpoint}{ContainerName}/{result.ResourceName}";
            
            return result;
        }
    }
}