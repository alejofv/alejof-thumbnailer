using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AlejoF.Media
{
    public static class AzureFunctions
    {
        private const string ThumbnailSignalQueue = "media-thumbnail-signal";
        private const string ResizeSignalQueue = "media-resize-signal";
        
        [FunctionName(nameof(Thumbnail))]
        public static async Task Thumbnail(
            [QueueTrigger(ThumbnailSignalQueue)]string queueItem,
            [Blob("{queueTrigger}", FileAccess.Read)] Stream input,
            [Queue(ResizeSignalQueue)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");

            var result = await new Functions.ThumbnailFunction().CreateThumbnail(queueItem, input, log);
            if (!result)
            {
                log.LogInformation($"Re-queuing for resizing: {queueItem}");
                await queueCollector.AddAsync(queueItem);
            }
        }

        [FunctionName(nameof(Resize))]
        public static async Task Resize(
            [QueueTrigger(ResizeSignalQueue)]string queueItem,
            [Blob("{queueTrigger}", FileAccess.Read)] Stream input,
            [Queue(ThumbnailSignalQueue)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");

            var result = await new Functions.ResizeFunction().ResizeImage(queueItem, input, log);
            if (result)
            {
                log.LogInformation($"Re-queuing for thumbnail generation: {queueItem}");
                await queueCollector.AddAsync(queueItem);
            }
        }
    }
}
