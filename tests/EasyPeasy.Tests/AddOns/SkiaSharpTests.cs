using System.Net;
using EasyPeasy.Attributes;
using EasyPeasy.SkiaSharp;
using EasyPeasy.Tests.Infrastructure;
using SkiaSharp;

namespace EasyPeasy.Tests.AddOns;

[Path("/images")]
public interface IImageApi
{
    [GET("/{name}"), Consumes(MediaType.ImagePNG)]
    Task<SKBitmap> GetBitmapAsync([PathParam] string name);

    [GET("/{name}"), Consumes(MediaType.ImagePNG)]
    Task<SKImage> GetImageAsync([PathParam] string name);

    [PUT("/{name}"), Produces(MediaType.ImageJPG)]
    Task UploadJpegAsync([PathParam] string name, SKBitmap image);

    [PUT("/{name}"), Produces(MediaType.ImageGIF)]
    Task UploadGifAsync([PathParam] string name, SKBitmap image);
}

public class SkiaSharpTests
{
    private static readonly EasyPeasySettings Settings = new()
    {
        MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault().AddSkiaSharpImages(),
    };

    [Fact]
    public async Task Images_are_decoded_as_bitmaps_and_images()
    {
        byte[] png = EncodedTestImage(SKEncodedImageFormat.Png);
        var handler = new StubHttpHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(png) { Headers = { ContentType = new(MediaType.ImagePNG) } },
        });
        var api = EasyPeasyClient.Create<IImageApi>(handler.CreateClient(), Settings);

        using var bitmap = await api.GetBitmapAsync("square");
        Assert.Equal(4, bitmap.Width);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));

        using var image = await api.GetImageAsync("square");
        Assert.Equal(3, image.Height);
    }

    [Fact]
    public async Task Bitmaps_are_encoded_in_the_Produces_format()
    {
        var handler = new StubHttpHandler();
        var api = EasyPeasyClient.Create<IImageApi>(handler.CreateClient(), Settings);
        using var bitmap = TestBitmap();

        await api.UploadJpegAsync("square", bitmap);

        Assert.Equal(MediaType.ImageJPG, handler.LastRequest.ContentType?.MediaType);
        using var decoded = SKBitmap.Decode(handler.LastRequest.Body);
        Assert.Equal(4, decoded.Width);
        Assert.Equal(SKEncodedImageFormat.Jpeg, SKCodec.Create(new SKMemoryStream(handler.LastRequest.Body)).EncodedFormat);
    }

    [Fact]
    public async Task Formats_skia_cannot_encode_are_reported()
    {
        var api = EasyPeasyClient.Create<IImageApi>(new StubHttpHandler().CreateClient(), Settings);
        using var bitmap = TestBitmap();

        await Assert.ThrowsAsync<NotSupportedException>(() => api.UploadGifAsync("square", bitmap));
    }

    private static SKBitmap TestBitmap()
    {
        var bitmap = new SKBitmap(4, 3);
        bitmap.Erase(SKColors.Red);
        return bitmap;
    }

    private static byte[] EncodedTestImage(SKEncodedImageFormat format)
    {
        using var bitmap = TestBitmap();
        using var data = bitmap.Encode(format, 100);
        return data.ToArray();
    }
}
