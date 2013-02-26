// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EasyPeasyFactory.cs">
//
//   The MIT License (MIT)
//     Copyright © 2013 Matt Channer (mchanner at gmail dot com)
//    
//     Permission is hereby granted, free of charge, to any person obtaining a 
//     copy of this software and associated documentation files (the “Software”),
//     to deal in the Software without restriction, including without limitation 
//     the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//     and/or sell copies of the Software, and to permit persons to whom the 
//     Software is furnished to do so, subject to the following conditions:
//   
//     The above copyright notice and this permission notice shall be included 
//     in all copies or substantial portions of the Software.
//   
//     THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//     THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
//     THE SOFTWARE.
// </copyright>
// <summary>
//   An implementation of the <see cref="IEasyPeasyFactory" /> interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

using EasyPeasy.Attributes;
using EasyPeasy.Client.Codecs;
using EasyPeasy.Client.Implementation;
using EasyPeasy.Client.Properties;

namespace EasyPeasy.Client
{
    /// <summary>
    /// An implementation of the <see cref="IEasyPeasyFactory"/> interface.
    /// </summary>
    public class EasyPeasyFactory : IEasyPeasyFactory
    {
        /// <summary> The attributes to apply to the new class </summary>
        private const TypeAttributes ClassAttributes =
            TypeAttributes.Class |
            TypeAttributes.AnsiClass |
            TypeAttributes.BeforeFieldInit;

        /// <summary> The binding flags for a protected method </summary>
        private const BindingFlags ProtectedMethodFlags =
            BindingFlags.NonPublic |
            BindingFlags.InvokeMethod |
            BindingFlags.Instance;

        /// <summary> Object used for locking critical sections </summary>
        private static readonly object Locker = new object();

        /// <summary> The dictionary of generated types </summary>
        private static readonly ConcurrentDictionary<string, Type> CachedTypes = new ConcurrentDictionary<string, Type>();

        /// <summary> The assembly builder (shared across instances to enable type caching). </summary>
        private static volatile AssemblyBuilder assemblyBuilder;

        /// <summary> The module builder (shared across instances to enable type caching). </summary>
        private static volatile ModuleBuilder moduleBuilder;

        /// <summary> The registry of serialization types. </summary>
        private readonly IMediaTypeHandlerRegistry registry;

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPeasyFactory"/> class.
        /// </summary>
        public EasyPeasyFactory()
        {
            registry = new DefaultMediaTypeRegistry();

            registry.RegisterMediaTypeHandler(MediaType.ApplicationXml,  new XmlMediaTypeHandler());
            registry.RegisterMediaTypeHandler(MediaType.TextXml,         new XmlMediaTypeHandler());
            registry.RegisterMediaTypeHandler(MediaType.ApplicationJson, new JsonMediaTypeHandler());
            registry.RegisterMediaTypeHandler(MediaType.TextHtml,        new PlainTextMediaTypeHandler());
            registry.RegisterMediaTypeHandler(MediaType.TextPlain,       new PlainTextMediaTypeHandler());
            registry.RegisterMediaTypeHandler(MediaType.ImageBMP,        new ImageMediaTypeHandler(ImageFormat.Bmp));
            registry.RegisterMediaTypeHandler(MediaType.ImageGIF,        new ImageMediaTypeHandler(ImageFormat.Gif));
            registry.RegisterMediaTypeHandler(MediaType.ImageJPG,        new ImageMediaTypeHandler(ImageFormat.Jpeg));
            registry.RegisterMediaTypeHandler(MediaType.ImagePNG,        new ImageMediaTypeHandler(ImageFormat.Png));
            registry.RegisterMediaTypeHandler(MediaType.ImageTIFF,       new ImageMediaTypeHandler(ImageFormat.Tiff));

            registry.RegisterCustomTypeHandler(typeof(byte[]),   new ByteArrayTypeHandler());
            registry.RegisterCustomTypeHandler(typeof(string),   new PlainTextMediaTypeHandler());
            registry.RegisterCustomTypeHandler(typeof(bool),     new ValueTypeHandler(TypeCode.Boolean));
            registry.RegisterCustomTypeHandler(typeof(byte),     new ValueTypeHandler(TypeCode.Byte));
            registry.RegisterCustomTypeHandler(typeof(char),     new ValueTypeHandler(TypeCode.Char));
            registry.RegisterCustomTypeHandler(typeof(DateTime), new ValueTypeHandler(TypeCode.DateTime));
            registry.RegisterCustomTypeHandler(typeof(decimal),  new ValueTypeHandler(TypeCode.Decimal));
            registry.RegisterCustomTypeHandler(typeof(double),   new ValueTypeHandler(TypeCode.Double));
            registry.RegisterCustomTypeHandler(typeof(short),    new ValueTypeHandler(TypeCode.Int16));
            registry.RegisterCustomTypeHandler(typeof(int),      new ValueTypeHandler(TypeCode.Int32));
            registry.RegisterCustomTypeHandler(typeof(long),     new ValueTypeHandler(TypeCode.Int64));
            registry.RegisterCustomTypeHandler(typeof(sbyte),    new ValueTypeHandler(TypeCode.SByte));
            registry.RegisterCustomTypeHandler(typeof(float),    new ValueTypeHandler(TypeCode.Single));
            registry.RegisterCustomTypeHandler(typeof(ushort),   new ValueTypeHandler(TypeCode.UInt16));
            registry.RegisterCustomTypeHandler(typeof(uint),     new ValueTypeHandler(TypeCode.UInt32));
            registry.RegisterCustomTypeHandler(typeof(ulong),    new ValueTypeHandler(TypeCode.UInt64));
            registry.RegisterCustomTypeHandler(typeof(FileInfo), new FileInfoTypeHandler());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPeasyFactory"/> class.
        /// </summary>
        /// <param name="registry"> The registry. </param>
        public EasyPeasyFactory(IMediaTypeHandlerRegistry registry)
        {
            Ensure.IsNotNull(registry, "registry");

            this.registry = registry;
        }

        /// <summary>
        /// Gets the registry of media type handlers used by each generated service to marshal types
        /// across the wire
        /// </summary>
        public IMediaTypeHandlerRegistry Registry
        {
            get
            {
                return registry;
            }
        }

        /// <summary>
        /// Creates a new implementation of the given service type, or returns an existing one if the
        /// type has previously been proxied.
        /// </summary>
        /// <param name="baseUri"> The base URI for the service being called. </param>
        /// <param name="credentials"> An optional <see cref="ICredentials"/> instance to be assigned to the
        /// underlying web request. </param>
        /// <typeparam name="TService"> The type of service to construct a proxy for.  Note that this must be
        /// an interface </typeparam>
        /// <returns> The created <see cref="TService"/> implementation. Note that this type will also
        /// implement the <see cref="IServiceClient"/> interface </returns>
        /// <exception cref="ArgumentNullException">Raised if baseUri is null</exception>
        /// <exception cref="ArgumentException">Raised if TService does not represent an interface</exception>
        /// <exception cref="ArgumentException">Raised if TService does not represent a public type</exception>
        public TService Create<TService>(Uri baseUri, ICredentials credentials = null) where TService : class
        {
            Type serviceType = typeof(TService);

            Ensure.IsNotNull(baseUri, "baseUri");
            Ensure.That(serviceType.IsInterface, Resources.ServiceMustBeAnInterfaceType);
            Ensure.That(serviceType.IsPublic, Resources.ServiceMustBePublic);

            EnsureBuilderIsInitialized();

            TService service;

            lock (Locker)
            {
                string implementationName = serviceType.Name + "<Impl>";
                Type implementationType = CachedTypes.GetOrAdd(
                    implementationName, _ => CreateProxyCore(serviceType, implementationName));

                ConstructorInfo ctor = implementationType.GetConstructor(Type.EmptyTypes);
                if (ctor == null) 
                    throw new EasyPeasyException(Resources.DefaultConstructorNotFoundOnNewType);

                service = (TService)ctor.Invoke(Type.EmptyTypes);

                IServiceClient serviceClient = (IServiceClient)service;
                serviceClient.BaseUri = baseUri;
                serviceClient.Credentials = credentials;

                serviceClient.MediaRegistry = registry;
            }

            return service;
        }

        /// <summary>
        /// Saves the assembly to disk
        /// </summary>
        /// <param name="assemblyPath">The path to save the assembly to</param>
        public void SaveGeneratedAssembly(FileInfo assemblyPath)
        {
            Ensure.IsNotNull(assemblyPath, "assemblyPath");
            if (assemblyPath.Exists)
                assemblyPath.Delete();

            assemblyBuilder.Save(assemblyPath.Name);
        }

        /// <summary>
        /// The core method for creating a new proxy type
        /// </summary>
        /// <param name="interfaceType"> The interface type. </param>
        /// <param name="implementationTypeName"> The implementation type name. </param>
        /// <returns> The created type </returns>
        private static Type CreateProxyCore(Type interfaceType, string implementationTypeName)
        {
            TypeBuilder classBuilder = CreateImplementationType(moduleBuilder, interfaceType, implementationTypeName);

            ILWriter.CreateConstructor(classBuilder, typeof(ServiceClient));

            foreach (MethodInfo method in interfaceType.GetMethods())
                GenerateMethod(classBuilder, method);

            return classBuilder.CreateType();
        }

        /// <summary>
        /// Generates the implementation for a single interface method
        /// </summary>
        /// <param name="classBuilder"> The class builder. </param>
        /// <param name="interfaceMethod"> The interface method. </param>
        private static void GenerateMethod(TypeBuilder classBuilder, MethodInfo interfaceMethod)
        {
            Type returnType = interfaceMethod.ReturnType;

            ParameterInfo[] parameters = interfaceMethod.GetParameters().ToArray();

            MethodBuilder methodBuilder = classBuilder.DefineMethod(
                    interfaceMethod.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    returnType,
                    parameters.Select(p => p.ParameterType).ToArray());

            // Assign names and attributes to each method parameter
            foreach (ParameterInfo t in parameters)
            {
                methodBuilder.DefineParameter(
                    t.Position + 1, // Parameter position is 1 based (0 == return value)
                    t.Attributes,
                    t.Name);
            }

            bool isGenericMethod = returnType.IsGenericType;
            bool isVoidMethod = returnType.IsVoid();

            string baseMethodName = DetermineBaseMethodInvocationName(returnType);

            MethodInfo baseMethodInfoPrototype = typeof(ServiceClient).GetMethod(baseMethodName, ProtectedMethodFlags);
            MethodInfo baseMethodHandler = baseMethodInfoPrototype;

            if (!isVoidMethod)
            {
                if (isGenericMethod)
                {
                    baseMethodHandler = baseMethodInfoPrototype.MakeGenericMethod(returnType.GetGenericArguments());
                }
                else if (returnType != typeof(Task) && returnType != typeof(WebResponse))
                {
                    // Return type is not one handled specifically by the underlying method being called, so make a generic
                    // version of the base method
                    baseMethodHandler = baseMethodInfoPrototype.MakeGenericMethod(returnType);
                }
            }

            ILGenerator il = methodBuilder.GetILGenerator();

            // Set up local variables
            // resultLocal will hold the result of calling the base class method - this is not set
            // if the return type is void
            LocalBuilder resultLocal = returnType.IsVoid() ? null : il.DeclareLocal(returnType);

            // Local to set up the method metadata array
            LocalBuilder metadataLocal = il.DeclareLocal(typeof(MethodMetadata));

            il.Emit(OpCodes.Ldarg_0); // Load `this` onto the stack

            ConstructorInfo metaCtor = typeof(MethodMetadata).GetConstructor(Type.EmptyTypes);
            if (metaCtor != null)
            {
                il.Emit(OpCodes.Newobj, metaCtor);
                il.Emit(OpCodes.Stloc, metadataLocal);
            }

            PopulateMetadata(il, metadataLocal, interfaceMethod);
            PopulateMetadataParameters(il, metadataLocal, interfaceMethod);

            // Load the metadata onto the stack and call the base method
            il.Emit(OpCodes.Ldloc, metadataLocal);
            il.Emit(OpCodes.Call, baseMethodHandler);

            // For non void results, store and load the result of the method call
            if (!returnType.IsVoid() && resultLocal != null)
            {
                il.Emit(OpCodes.Stloc, resultLocal);
                il.Emit(OpCodes.Ldloc, resultLocal);
            }

            il.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// The populate metadata.
        /// </summary>
        /// <param name="il">The IL Generator</param>
        /// <param name="metadataLocal"> The metadata local. </param>
        /// <param name="interfaceMethod"> The interface method. </param>
        private static void PopulateMetadataParameters(ILGenerator il, LocalBuilder metadataLocal, MethodInfo interfaceMethod)
        {
            ParameterInfo[] parameters = interfaceMethod.GetParameters();

            Type methodMetaType = typeof(MethodMetadata);

            foreach (ParameterInfo parameter in parameters)
            {
                if (parameter.IsOut)
                    throw new NotSupportedException(Resources.OutAndOptionalParamsNotSupported);

                bool attributeFound = false;

                HeaderParamAttribute headerAttribute = ReflectionUtils.GetAttribute<HeaderParamAttribute>(parameter);
                if (headerAttribute != null)
                {
                    attributeFound = true;
                    ILWriter.AssignParameterValueToDictionaryProperty(
                        il,
                        metadataLocal,
                        methodMetaType,
                        "Headers",
                        headerAttribute.HeaderName,
                        parameter);
                }

                QueryParamAttribute queryAttribute = ReflectionUtils.GetAttribute<QueryParamAttribute>(parameter);
                if (queryAttribute != null)
                {
                    attributeFound = true;
                    ILWriter.AssignParameterValueToDictionaryProperty(
                        il,
                        metadataLocal,
                        methodMetaType,
                        "QueryParameters",
                        queryAttribute.ParameterName,
                        parameter);
                }

                PathParamAttribute pathAttribute = ReflectionUtils.GetAttribute<PathParamAttribute>(parameter);
                if (pathAttribute != null)
                {
                    attributeFound = true;
                    ILWriter.AssignParameterValueToDictionaryProperty(
                        il,
                        metadataLocal,
                        methodMetaType,
                        "PathParameters",
                        pathAttribute.ParameterName,
                        parameter);
                }

                if (!attributeFound)
                {
                    MethodInfo bodySetter = methodMetaType.GetProperty("RequestBody").GetSetMethod();

                    il.Emit(OpCodes.Ldloc, metadataLocal);
                    il.Emit(OpCodes.Ldarg, parameter.Position + 1);
                    if (parameter.ParameterType.IsValueType)
                        il.Emit(OpCodes.Box, parameter.ParameterType);

                    il.Emit(OpCodes.Call, bodySetter);
                }
            }
        }

        /// <summary>
        /// Populates the high level metadata properties
        /// </summary>
        /// <param name="il">The IL Generator</param>
        /// <param name="metadataLocal"> The metadata local. </param>
        /// <param name="interfaceMethod"> The interface method. </param>
        private static void PopulateMetadata(ILGenerator il, LocalBuilder metadataLocal, MethodInfo interfaceMethod)
        {
            HttpVerb verb = ReflectionUtils.DetermineHttpVerb(interfaceMethod);

            string consumeMediaType = ReflectionUtils.DetermineConsumesMediaType(interfaceMethod, MediaType.TextXml);
            string producesMediaType = ReflectionUtils.DetermineProducesMediaType(interfaceMethod, MediaType.TextXml);
            
            PathAttribute methodPathAttr = ReflectionUtils.GetAttribute<PathAttribute>(interfaceMethod);
            PathAttribute servicePathAttr = ReflectionUtils.GetAttribute<PathAttribute>(interfaceMethod.DeclaringType);

            string methodPath = methodPathAttr == null ? "/" : methodPathAttr.Path;
            string servicePath = servicePathAttr == null ? string.Empty : servicePathAttr.Path;

            Type methodMetaType = typeof(MethodMetadata);

            ILWriter.WritePropertyString(il, metadataLocal, methodMetaType, "ServicePath", servicePath);
            ILWriter.WritePropertyString(il, metadataLocal, methodMetaType, "MethodPath", methodPath);
            ILWriter.WritePropertyString(il, metadataLocal, methodMetaType, "Consumes", consumeMediaType);
            ILWriter.WritePropertyString(il, metadataLocal, methodMetaType, "Produces", producesMediaType);
            ILWriter.WritePropertyInt(il, metadataLocal, methodMetaType, "Verb", (int)verb);
        }

        /// <summary>
        /// Determines the underlying method to call on the service client based on the
        /// expected return type of the method being proxied.
        /// </summary>
        /// <param name="returnType">The return type of the method</param>
        /// <returns>The name of the method to be called</returns>
        private static string DetermineBaseMethodInvocationName(Type returnType)
        {
            if (returnType.IsGenericType)
            {
                Type genericDefinition = returnType.GetGenericTypeDefinition();

                bool isAsyncRequest = genericDefinition == typeof(Task<>);

                if (isAsyncRequest)
                {
                    // If response is a raw response, no conversion is needed, so use the raw method
                    return genericDefinition == typeof(WebResponse)
                        ? "AsyncRequestWithRawResponse"
                        : "AsyncRequestWithResult";
                }

                return "SyncRequestWithResult";
            }
            else if (returnType.IsVoid())
            {
                return "SyncVoidRequest";
            }
            else
            {
                bool isAsyncRequest = returnType == typeof(Task);
                bool isRawRequest = returnType == typeof(WebResponse);

                if (isRawRequest) return "SyncRequestWithRawResponse";
                return isAsyncRequest ? "AsyncVoidRequest" : "SyncRequestWithResult";
            }
        }

        /// <summary>
        /// Constructs a type builder which generates a class type implementing
        /// the given interface
        /// </summary>
        /// <param name="modBuilder"> The module builder. </param>
        /// <param name="interfaceType"> The interface type. </param>
        /// <param name="typeName"> The type name. </param>
        /// <returns> The <see cref="TypeBuilder"/>. </returns>
        /// <exception cref="InvalidOperationException">Thrown when interfaceType does not represent an interface </exception>
        private static TypeBuilder CreateImplementationType(
            ModuleBuilder modBuilder,
            Type interfaceType,
            string typeName)
        {
            // The new type inherits from RestClient and implements the given interface
            TypeBuilder typeBuilder = modBuilder.DefineType(
                typeName,
                ClassAttributes,
                typeof(ServiceClient),
                new[] { interfaceType });

            return typeBuilder;
        }

        /// <summary>
        /// Ensures there is a valid instance of the assembly and module builder
        /// created (as static references)
        /// </summary>
        private static void EnsureBuilderIsInitialized()
        {
            // Double checked locking
            if (assemblyBuilder == null)
            {
                lock (Locker)
                {
                    if (assemblyBuilder == null)
                    {
                        var builders = CreateBuilders();
                        assemblyBuilder = builders.Item1;
                        moduleBuilder = builders.Item2;
                    }
                }
            }
        }

        /// <summary>
        /// Constructs an assembly and module builder
        /// </summary>
        /// <returns>
        /// The <see cref="Tuple"/> containing both builders..
        /// </returns>
        private static Tuple<AssemblyBuilder, ModuleBuilder> CreateBuilders()
        {
            AssemblyName name = new AssemblyName("DynamicServiceAssembly");

            AppDomain thisDomain = Thread.GetDomain();

            AssemblyBuilder asmBuilder = thisDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule(asmBuilder.GetName().Name, "DynamicServiceAssembly.dll");

            return Tuple.Create(asmBuilder, modBuilder);
        }
    }
}
