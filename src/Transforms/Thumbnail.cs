using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

namespace AlejoF.Thumbnailer.Transforms
{
    public class Thumbnail : ITransform
    {
        private readonly ILogger _log;
        private readonly FunctionSettings _settings;

        public Thumbnail(
            ILogger log,
            Settings.FunctionSettings settings)
        {
            this._log = log;
            this._settings = settings;
        }

        public async Task<bool> Execute(Stream input, string blobName, string outputBlobName)
        {
            // Validate max width of the image
            using (Image image = Image.Load(input))
            {
                if (image.Width > _settings.MaxMediaWidth)
                {
                    // Enqueue for resizing
                    _log.LogInformation($"Image is too big ({blobName}). Requesting resize.");
                    return false;
                }
            }

            var blockBlob = Helpers.BlobHelper.GetBlobReference(_settings.StorageConnectionString, outputBlobName);
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
    }
}
