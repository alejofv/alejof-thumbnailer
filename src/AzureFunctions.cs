using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AlejoF.Media
{
    public static class AzureFunctions
    {
        [FunctionName("MediaThumbnailer")]
        public static void Run(
            [QueueTrigger("media-thumbnail-signal")]string queueItem,
            [Blob("{queueTrigger}", FileAccess.Read)] Stream input,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");
        }
    }
}
