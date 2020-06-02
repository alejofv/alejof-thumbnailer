using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AlejoF.Thumbnailer.Transforms
{
    public class Resize
    {
        private readonly FunctionSettings _settings;

        public Resize(
            Settings.FunctionSettings settings)
        {
            this._settings = settings;
        }

        public async Task<(bool Success, string? Message)> Execute(Stream input, string blobName, string outputBlobName)
        {
            var encoder = Helpers.EncoderFactory.GetEncoder(Path.GetExtension(blobName));
            if (encoder == null)
                return (false, $"Image type for blob '{blobName}' not supported");

            var blockBlob = Helpers.BlobHelper.GetBlobReference(outputBlobName);
            if (blockBlob == null)
                return (false, $"Blob reference '{outputBlobName}' not valid");

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

            return (true, null);
        }
    }
}
