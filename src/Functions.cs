using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AlejoF.Thumbnailer
{
    public static class AzureFunctions
    {
        private const string ThumbnailSignalQueue = "media-thumbnail-signal";
        private const string ResizeSignalQueue = "media-resize-signal";
        private const string StorageConnectionString = "StorageConnectionString";
        
        [FunctionName(nameof(Thumbnail))]
        public static async Task Thumbnail(
            [QueueTrigger(ThumbnailSignalQueue, Connection = StorageConnectionString)]string blobName,
            [Blob("{queueTrigger}", FileAccess.Read)] Stream input,
            [Queue(ResizeSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var extension = System.IO.Path.GetExtension(blobName);
            var outputName = blobName.Remove(blobName.Length - extension.Length) + "-thumb" + extension;

            var result = await new Transforms.Thumbnail(log, Settings.Factory.Build())
                .Execute(input, blobName, outputName);

            if (!result)
            {
                log.LogInformation($"Re-queuing for resizing: {blobName}");
                await queueCollector.AddAsync(blobName);
            }
        }

        [FunctionName(nameof(Resize))]
        public static async Task Resize(
            [QueueTrigger(ResizeSignalQueue, Connection = StorageConnectionString)]string blobName,
            [Blob("{queueTrigger}", FileAccess.Read)] Stream input,
            [Queue(ThumbnailSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var result = await new Transforms.Resize(log, Settings.Factory.Build())
                .Execute(input, blobName, blobName);

            if (result)
            {
                log.LogInformation($"Re-queuing for thumbnail generation: {blobName}");
                await queueCollector.AddAsync(blobName);
            }
        }
    }
}
