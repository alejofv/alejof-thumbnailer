using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AlejoF.Thumbnailer
{
    public class Functions
    {
        private const string ThumbnailSignalQueue = "media-thumbnail-signal";
        private const string ResizeSignalQueue = "media-resize-signal";
        private const string CleanupSignalQueue = "media-cleanup-signal";
        private const string ResizedNameToken = "__resized__";
        public const string StorageConnectionString = "StorageConnectionString";

        private readonly FunctionSettings settings;

        public Functions(Settings.FunctionSettings settings)
        {
            this.settings = settings;
        }

        [FunctionName(nameof(Thumbnail))]
        public async Task Thumbnail(
            [QueueTrigger(ThumbnailSignalQueue, Connection = StorageConnectionString)]string blobName,
            [Blob("{queueTrigger}", FileAccess.Read, Connection = StorageConnectionString)] Stream input,
            [Queue(ResizeSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> resizeQueueCollector,
            [Queue(CleanupSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> cleanupQueueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var extension = System.IO.Path.GetExtension(blobName);
            var outputName = blobName.Remove(blobName.Length - extension.Length) + "-thumb" + extension;

            // Cleanup name and blob for resized images
            var wasResized = blobName.Contains(ResizedNameToken);
            if (wasResized)
                outputName = outputName.Replace(ResizedNameToken, string.Empty);

            var result = await new Transforms.Thumbnail(settings)
                .Execute(input, blobName, outputName);

            if (!result.Success)
            {
                log.LogInformation(result.Message);
                await resizeQueueCollector.AddAsync(blobName);
            }

            if (wasResized)
            {
                log.LogInformation(result.Message);
                await cleanupQueueCollector.AddAsync(blobName);
            }
        }

        [FunctionName(nameof(Resize))]
        public async Task Resize(
            [QueueTrigger(ResizeSignalQueue, Connection = StorageConnectionString)]string blobName,
            [Blob("{queueTrigger}", FileAccess.Read, Connection = StorageConnectionString)] Stream input,
            [Queue(ThumbnailSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var extension = System.IO.Path.GetExtension(blobName);
            var outputName = blobName.Remove(blobName.Length - extension.Length) + ResizedNameToken + extension;

            var result = await new Transforms.Resize(settings)
                .Execute(input, blobName, outputName);

            if (result.Success)
            {
                log.LogInformation($"Re-queuing for thumbnail generation: {outputName}");
                await queueCollector.AddAsync(outputName);
            }
        }

        [FunctionName(nameof(Cleanup))]
        public async Task Cleanup(
            [QueueTrigger(CleanupSignalQueue, Connection = StorageConnectionString)]string blobName,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var blockBlob = Helpers.BlobHelper.GetBlobReference(blobName);
            if (blockBlob != null)
                await blockBlob.DeleteIfExistsAsync();
        }
    }
}
