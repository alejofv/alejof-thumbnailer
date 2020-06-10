using System;
using System.IO;
using System.Threading.Tasks;
using AlejoF.Thumbnailer.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
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

        private const string UploadFilenameHeaderName = "x-blob-filename";
        private const string RequestThumbnailPathHeaderName = "x-blob-path";

        private readonly FunctionSettings settings;

        public Functions(Settings.FunctionSettings settings)
        {
            this.settings = settings;
        }

        [FunctionName(nameof(Upload))]
        public IActionResult Upload(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "upload")]HttpRequest req,
            ILogger log)
        {
            log.LogInformation($"C# Http trigger function processed: {nameof(Upload)}");

            var filename = (string)req.Headers[UploadFilenameHeaderName];
            if (string.IsNullOrEmpty(filename))
                return new BadRequestResult();

            var result = new Handlers.Upload(this.settings)
                .Handle(filename);

            return new OkObjectResult(new { url = result.Url, token = result.Token });
        }

        [FunctionName(nameof(RequestThumbnail))]
        public async Task<IActionResult> RequestThumbnail(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "thumbnail")]HttpRequest req,
            [Queue(ThumbnailSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Http trigger function processed: {nameof(RequestThumbnail)}");

            var path = (string)req.Headers[RequestThumbnailPathHeaderName];
            if (string.IsNullOrEmpty(path))
                return new BadRequestResult();

            await queueCollector.AddAsync(path);

            return new OkResult();
        }

        [FunctionName(nameof(ProcessThumbnail))]
        public async Task ProcessThumbnail(
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

            var result = await new Handlers.Thumbnail(settings)
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

        [FunctionName(nameof(ProcessResize))]
        public async Task ProcessResize(
            [QueueTrigger(ResizeSignalQueue, Connection = StorageConnectionString)]string blobName,
            [Blob("{queueTrigger}", FileAccess.Read, Connection = StorageConnectionString)] Stream input,
            [Queue(ThumbnailSignalQueue, Connection = StorageConnectionString)]IAsyncCollector<string> queueCollector,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {blobName}");

            var extension = System.IO.Path.GetExtension(blobName);
            var outputName = blobName.Remove(blobName.Length - extension.Length) + ResizedNameToken + extension;

            var result = await new Handlers.Resize(settings)
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
