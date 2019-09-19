using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using SixLabors.ImageSharp;

namespace AlejoF.Media.Functions
{
    public class ThumbnailFunction
    {
        private readonly Settings.FunctionSettings _settings = Settings.Factory.Build();
        
        public async Task<bool> CreateThumbnail(string blobName, Stream input, ILogger log)
        {
            // Validate max width of the image
            using (Image image = Image.Load(input))
            {
                if (image.Width > _settings.MaxMediaWidth)
                {
                    // Enqueue for resizing
                    log.LogInformation($"Image is too big ({blobName}). Requesting resize.");
                    return false;
                }
            }

            // append "_thumbnail" to name
            var extension = Path.GetExtension(blobName);
            var newBlobName = $"{blobName}.thumbnail{extension}";

            var storageAccount = CloudStorageAccount.Parse(_settings.StorageConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("note-media");
            var blockBlob = container.GetBlockBlobReference(newBlobName);

            try
            {
                input.Position = 0;
                
                // Get smart thumbnail
                var credentials = new ApiKeyServiceClientCredentials(_settings.CognitiveServices.Key);
                using (var visionClient = new ComputerVisionClient(credentials) { Endpoint = _settings.CognitiveServices.Host })
                using (var result = await visionClient.GenerateThumbnailInStreamAsync(_settings.ThumbnailSize, _settings.ThumbnailSize, input, true))
                using (var output = new MemoryStream())
                {
                    await result.CopyToAsync(output);

                    output.Position = 0;
                    await blockBlob.UploadFromStreamAsync(output);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Error in {nameof(CreateThumbnail)}: {ex}");
                throw;
            }
        }
    }
}
