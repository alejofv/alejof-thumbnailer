using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AlejoF.Media.Helpers
{
    public static class BlobContainerHelper    
    {        
        public static CloudBlockBlob GetBlobReference(string connectionString, string path)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var containerName = path.Split('/')[0];
            var container = blobClient.GetContainerReference(containerName);

            var blobName = path.Replace($"{containerName}/", string.Empty);
            return container.GetBlockBlobReference(blobName);
        }
    }
}
