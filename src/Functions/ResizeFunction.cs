using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AlejoF.Media.Functions
{
    public class ResizeFunction
    {
        private readonly Settings.FunctionSettings _settings = Settings.Factory.Build();
        
        public async Task<bool> ResizeImage(string blobName, Stream input, ILogger log)
        {
            var extension = Path.GetExtension(blobName).Replace(".", string.Empty);
            var encoder = Helpers.EncoderHelper.GetEncoder(extension);

            if (encoder == null)
            {
                log.LogInformation($"No encoder support for: {blobName}");
                return false;
            }

            var blockBlob = Helpers.BlobContainerHelper.GetBlobReference(_settings.StorageConnectionString, blobName);
            try
            {
                input.Position = 0;
                
                using (var output = new MemoryStream())
                using (Image image = Image.Load(input))
                {
                    var newWidth = _settings.MaxMediaWidth - 10;

                    var divisor = image.Width / newWidth;
                    var height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                    image.Mutate(x => x.Resize(newWidth, height));
                    image.Save(output, encoder);

                    output.Position = 0;
                    await blockBlob.UploadFromStreamAsync(output);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Error in {nameof(ResizeImage)}: {ex}");
                throw;
            }
        }
    }
}
