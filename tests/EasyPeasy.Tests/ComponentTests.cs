using System.Text;
using EasyPeasy.Handlers;
using EasyPeasy.Runtime;

namespace EasyPeasy.Tests;

public class ComponentTests
{
    [Theory]
    [InlineData("application/json", typeof(JsonMediaTypeHandler))]
    [InlineData("APPLICATION/JSON", typeof(JsonMediaTypeHandler))]
    [InlineData("application/problem+json", typeof(JsonMediaTypeHandler))]
    [InlineData("text/plain", typeof(PlainTextMediaTypeHandler))]
    [InlineData("text/csv", typeof(PlainTextMediaTypeHandler))]
    public void Registry_resolves_media_types(string mediaType, Type expected)
    {
        var registry = MediaTypeHandlerRegistry.CreateDefault();

        Assert.True(registry.TryGetHandler(mediaType, out var handler));
        Assert.IsType(expected, handler);
    }

    [Fact]
    public void Registry_does_not_guess_unknown_media_types()
    {
        Assert.False(MediaTypeHandlerRegistry.CreateDefault().TryGetHandler("application/xml", out _));
    }

    [Fact]
    public void Registered_handlers_replace_defaults()
    {
        var custom = new PlainTextMediaTypeHandler();
        var registry = MediaTypeHandlerRegistry.CreateDefault().Register(MediaType.ApplicationJson, custom);

        Assert.True(registry.TryGetHandler(MediaType.ApplicationJson, out var handler));
        Assert.Same(custom, handler);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("s", "s")]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(1.25, "1.25")]
    [InlineData(DayOfWeek.Monday, "Monday")]
    public void Formatter_formats_values(object? value, string? expected)
    {
        Assert.Equal(expected, DefaultUrlParameterFormatter.Instance.Format(value));
    }

    [Fact]
    public void Formatter_uses_iso_8601_for_dates()
    {
        var formatter = DefaultUrlParameterFormatter.Instance;

        Assert.Equal("2026-09-23T01:02:03.0000000Z", formatter.Format(new DateTime(2026, 9, 23, 1, 2, 3, DateTimeKind.Utc)));
        Assert.Equal("2026-09-23", formatter.Format(new DateOnly(2026, 9, 23)));
        Assert.Equal("13:45:00", formatter.Format(new TimeOnly(13, 45)));
    }

    [Fact]
    public async Task Plain_text_handler_converts_enums_guids_and_dates()
    {
        var handler = new PlainTextMediaTypeHandler();
        var guid = Guid.NewGuid();

        Assert.Equal(DayOfWeek.Friday, await handler.DeserializeAsync(new StringContent("friday"), typeof(DayOfWeek), default));
        Assert.Equal(guid, await handler.DeserializeAsync(new StringContent(guid.ToString()), typeof(Guid), default));
        Assert.Equal(
            new DateTimeOffset(2026, 9, 23, 0, 0, 0, TimeSpan.Zero),
            await handler.DeserializeAsync(new StringContent("2026-09-23T00:00:00Z"), typeof(DateTimeOffset), default));
    }

    [Fact]
    public async Task Plain_text_handler_writes_invariant_text()
    {
        using var content = new PlainTextMediaTypeHandler().Serialize(1.5, typeof(double), MediaType.TextPlain);

        Assert.Equal("1.5", await content.ReadAsStringAsync());
        Assert.Equal(Encoding.UTF8.WebName, content.Headers.ContentType?.CharSet);
    }

    [Fact]
    public void Templates_reject_unclosed_variables()
    {
        Assert.Throws<ArgumentException>(() => new RequestTemplate(HttpMethod.Get, "/items/{id"));
    }
}
