using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using SixLabors.ImageSharp;

namespace AlejoF.Thumbnailer.Handlers
{
    public class Thumbnail
    {
        private readonly FunctionSettings _settings;

        public Thumbnail(
            Settings.FunctionSettings settings)
        {
            this._settings = settings;
        }

        public async Task<(bool Success, string? Message)> Execute(Stream input, string blobName, string outputBlobName)
        {
            // Validate max width of the image
            using (Image image = Image.Load(input))
            {
                if (image.Width > _settings.MaxMediaSize)
                    return (false, $"Image is too big. Requesting resize.");
            }

            var blockBlob = Helpers.BlobHelper.GetBlobReference(outputBlobName) ?? throw new ArgumentException($"Blob '{blobName}' not found");
            input.Position = 0;

            // Get smart thumbnail
            var credentials = new ApiKeyServiceClientCredentials(_settings.CognitiveServices.Key);
            using (var visionClient = new ComputerVisionClient(credentials) { Endpoint = _settings.CognitiveServices.Endpoint })
            using (var result = await visionClient.GenerateThumbnailInStreamAsync(_settings.ThumbnailSize, _settings.ThumbnailSize, input, true))
            using (var output = new MemoryStream())
            {
                await result.CopyToAsync(output);

                output.Position = 0;
                await blockBlob.UploadFromStreamAsync(output);
            }

            return (true, null);
        }
    }
}
