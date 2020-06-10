using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.WindowsAzure.Storage.Blob;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace AlejoF.Thumbnailer.Handlers
{
    public class Upload
    {
        private readonly FunctionSettings _settings;

        public Upload(
            Settings.FunctionSettings settings)
        {
            this._settings = settings;
        }

        public (string Url, string Token) Handle(string filename)
        {
            var blobPath = $"{_settings.UploadContainer}/{filename}";
            var blob = Helpers.BlobHelper.GetBlobReference(blobPath) ?? throw new ArgumentException($"Blob '{blobPath}' not valid");

            // Create a new access policy and define its constraints.
            var adHocSAS = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Create
            };

            // Generate the shared access signature on the blob, setting the constraints directly on the signature.
            var sasBlobToken = blob.GetSharedAccessSignature(adHocSAS);

            return (blob.Uri.ToString(), sasBlobToken);
        }
    }
}
