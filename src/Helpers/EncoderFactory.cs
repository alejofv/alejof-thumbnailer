using System.Text.RegularExpressions;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace AlejoF.Thumbnailer.Helpers
{
    internal static class EncoderFactory
    {
        internal static IImageEncoder GetEncoder(string extension)
        {
            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);
            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        return new PngEncoder();
                    case "jpg":
                    case "jpeg":
                        return new JpegEncoder();
                    case "gif":
                        return new GifEncoder();
                }
            }

            return null;
        }
    }
}
