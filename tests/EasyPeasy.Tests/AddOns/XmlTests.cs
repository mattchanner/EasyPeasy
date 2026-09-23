using System.Runtime.Serialization;
using EasyPeasy.Attributes;
using EasyPeasy.Tests.Infrastructure;
using EasyPeasy.Xml;

namespace EasyPeasy.Tests.AddOns;

[DataContract(Name = "order", Namespace = "")]
public sealed class Order
{
    [DataMember(Name = "id", Order = 1)]
    public int Id { get; set; }

    [DataMember(Name = "item", Order = 2)]
    public string? Item { get; set; }
}

[Path("/orders"), Consumes(MediaType.ApplicationXml), Produces(MediaType.ApplicationXml)]
public interface IOrderApi
{
    [GET("/{id}")]
    Task<Order> GetAsync([PathParam] int id);

    [POST]
    Task CreateAsync(Order order);
}

public class XmlTests
{
    private static readonly EasyPeasySettings Settings = new() { MediaTypeHandlers = MediaTypeHandlerRegistry.CreateDefault().AddXml() };

    [Fact]
    public async Task Xml_responses_are_deserialized()
    {
        var handler = StubHttpHandler.Text("<order><id>7</id><item>Tea</item></order>", mediaType: "application/xml");
        var api = EasyPeasyClient.Create<IOrderApi>(handler.CreateClient(), Settings);

        var order = await api.GetAsync(7);

        Assert.Equal(7, order.Id);
        Assert.Equal("Tea", order.Item);
        Assert.Equal("application/xml", handler.LastRequest.Header("Accept"));
    }

    [Fact]
    public async Task Xml_bodies_are_serialized()
    {
        var handler = new StubHttpHandler();
        var api = EasyPeasyClient.Create<IOrderApi>(handler.CreateClient(), Settings);

        await api.CreateAsync(new Order { Id = 3, Item = "Cake" });

        Assert.Equal("application/xml", handler.LastRequest.ContentType?.MediaType);
        Assert.Contains("<id>3</id><item>Cake</item>", handler.LastRequest.BodyText, StringComparison.Ordinal);
    }

    [Fact]
    public void Structured_xml_media_types_use_the_xml_handler()
    {
        var registry = MediaTypeHandlerRegistry.CreateDefault().AddXml();

        Assert.True(registry.TryGetHandler("application/atom+xml", out var handler));
        Assert.IsType<XmlMediaTypeHandler>(handler);
        Assert.True(registry.TryGetHandler("text/xml", out _));
    }
}
