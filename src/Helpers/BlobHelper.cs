using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AlejoF.Thumbnailer.Helpers
{
    public static class BlobHelper    
    {        
        public static CloudBlockBlob? GetBlobReference(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            var connectionString = Environment.GetEnvironmentVariable(Functions.StorageConnectionString, EnvironmentVariableTarget.Process);
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
