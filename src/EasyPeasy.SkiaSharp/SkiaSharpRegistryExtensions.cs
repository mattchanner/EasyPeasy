using SkiaSharp;

namespace EasyPeasy.SkiaSharp;

/// <summary>Adds SkiaSharp image support to a <see cref="MediaTypeHandlerRegistry"/>.</summary>
public static class SkiaSharpRegistryExtensions
{
    /// <summary>
    /// Registers image handlers for PNG, JPEG, WebP, GIF and BMP. All five can be read; SkiaSharp can only
    /// encode PNG, JPEG and WebP, so sending a GIF or BMP body throws <see cref="NotSupportedException"/>.
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <param name="quality">The encoding quality for JPEG and WebP, from 0 to 100.</param>
    /// <returns>The registry, for chaining.</returns>
    public static MediaTypeHandlerRegistry AddSkiaSharpImages(this MediaTypeHandlerRegistry registry, int quality = 100)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return registry
            .Register(MediaType.ImagePNG, new SkiaImageMediaTypeHandler(SKEncodedImageFormat.Png, quality))
            .Register(MediaType.ImageJPG, new SkiaImageMediaTypeHandler(SKEncodedImageFormat.Jpeg, quality))
            .Register(MediaType.ImageWebP, new SkiaImageMediaTypeHandler(SKEncodedImageFormat.Webp, quality))
            .Register(MediaType.ImageGIF, new SkiaImageMediaTypeHandler(SKEncodedImageFormat.Gif, quality))
            .Register(MediaType.ImageBMP, new SkiaImageMediaTypeHandler(SKEncodedImageFormat.Bmp, quality));
    }
}
