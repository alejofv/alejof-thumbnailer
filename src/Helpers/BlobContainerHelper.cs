using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AlejoF.Media.Helpers
{
    public static class BlobContainerHelper    
    {
        private const string _containerName = "note-media";
        
        public static CloudBlockBlob GetBlobReference(string connectionString, string name)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);
            
            return container.GetBlockBlobReference(name);
        }
    }
}
