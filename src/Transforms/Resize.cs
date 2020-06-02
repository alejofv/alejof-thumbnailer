using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AlejoF.Thumbnailer.Transforms
{
    public class Resize : ITransform
    {
        private readonly ILogger _log;
        private readonly FunctionSettings _settings;

        public Resize(
            ILogger log,
            Settings.FunctionSettings settings)
        {
            this._log = log;
            this._settings = settings;
        }

        public async Task<bool> Execute(Stream input, string blobName, string outputBlobName)
        {
            var extension = Path.GetExtension(blobName).Replace(".", string.Empty);
            var encoder = Helpers.EncoderFactory.GetEncoder(extension);

            if (encoder == null)
            {
                _log.LogInformation($"No encoder support for: {blobName}");
                return false;
            }

            var blockBlob = Helpers.BlobHelper.GetBlobReference(_settings.StorageConnectionString, outputBlobName);
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
    }
}
