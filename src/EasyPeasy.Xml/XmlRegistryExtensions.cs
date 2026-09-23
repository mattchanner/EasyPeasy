using System.Diagnostics.CodeAnalysis;

namespace EasyPeasy.Xml;

/// <summary>Adds XML support to a <see cref="MediaTypeHandlerRegistry"/>.</summary>
public static class XmlRegistryExtensions
{
    /// <summary>
    /// Registers <see cref="XmlMediaTypeHandler"/> for <c>application/xml</c> and <c>text/xml</c>
    /// (and so for any <c>+xml</c> media type).
    /// </summary>
    /// <param name="registry">The registry.</param>
    /// <returns>The registry, for chaining.</returns>
    [RequiresUnreferencedCode(XmlMediaTypeHandler.TrimmingMessage)]
    [RequiresDynamicCode(XmlMediaTypeHandler.TrimmingMessage)]
    public static MediaTypeHandlerRegistry AddXml(this MediaTypeHandlerRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var handler = new XmlMediaTypeHandler();
        return registry
            .Register(MediaType.ApplicationXml, handler)
            .Register(MediaType.TextXml, handler);
    }
}
