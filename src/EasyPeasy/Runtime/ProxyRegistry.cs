using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasyPeasy.Runtime;

/// <summary>
/// Maps interfaces to the client implementations generated for them at compile time.
/// Generated code registers each implementation from a module initializer.
/// </summary>
/// <remarks>This type supports generated code and is not intended to be used directly.</remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class ProxyRegistry
{
    private static readonly ConcurrentDictionary<Type, Func<ClientContext, object>> Factories = new();

    /// <summary>Registers the generated implementation of <typeparamref name="TInterface"/>.</summary>
    /// <typeparam name="TInterface">The service interface.</typeparam>
    /// <param name="factory">Creates the implementation.</param>
    public static void Register<TInterface>(Func<ClientContext, TInterface> factory)
        where TInterface : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        Factories[typeof(TInterface)] = factory;
    }

    internal static TInterface Create<TInterface>(ClientContext context)
        where TInterface : class
    {
        if (!Factories.TryGetValue(typeof(TInterface), out var factory))
        {
            // The module initializer of the interface's assembly has not run if nothing from that
            // assembly has executed yet (only its types have been referenced), so run it now.
            RuntimeHelpers.RunModuleConstructor(typeof(TInterface).Module.ModuleHandle);
            if (!Factories.TryGetValue(typeof(TInterface), out factory))
            {
                throw new EasyPeasyException(
                    $"No EasyPeasy client was generated for '{typeof(TInterface)}'. Clients are generated at compile time for " +
                    "interfaces whose methods have an HTTP method attribute such as [GET], in projects that reference the " +
                    "EasyPeasy package. Check the build output for EasyPeasy (EP) diagnostics.");
            }
        }

        return (TInterface)factory(context);
    }
}
