namespace AlejoF.Thumbnailer
{
    public class ThumbnailRequest
    {
        public string BlobPath { get; set; } = string.Empty;
        public string ThumbnailBlobPath { get; set; } = string.Empty;
    }

    public class ResizeRequest
    {
        public string OriginalBlobPath { get; set; } = string.Empty;
        public string ResizedBlobPath { get; set; } = string.Empty;
        public string ThumbnailBlobPath { get; set; } = string.Empty;
    }
}