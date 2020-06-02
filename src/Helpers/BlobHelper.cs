using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AlejoF.Thumbnailer.Helpers
{
    public static class BlobHelper    
    {        
        public static CloudBlockBlob GetBlobReference(string connectionString, string path)
        {
            var containerName = path.Split('/')[0];
            var blobName = path.Replace($"{containerName}/", string.Empty);

            return CloudStorageAccount
                .Parse(connectionString)
                .CreateCloudBlobClient()
                .GetContainerReference(containerName)
                .GetBlockBlobReference(blobName);
        }
    }
}
