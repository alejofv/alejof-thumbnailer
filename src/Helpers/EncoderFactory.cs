using System.Text.RegularExpressions;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace AlejoF.Thumbnailer.Helpers
{
    internal static class EncoderFactory
    {
        internal static IImageEncoder? GetEncoder(string extension) =>
            (extension.Replace(".", string.Empty)) switch
            {
                "png"  => new PngEncoder(),
                "jpg"  => new JpegEncoder(),
                "jpeg" => new JpegEncoder(),
                "gif"  => new GifEncoder(),
                _ => null,
            };
    }
}
