// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEasyPeasyFactory.cs">
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
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net;

namespace EasyPeasy
{
    /// <summary>
    /// The easy peasy factory is the main work horse for building new REST client
    /// types from decorated interfaces.
    /// </summary>
    public interface IEasyPeasyFactory
    {
        /// <summary>
        /// Gets the registry of media type handlers used by each generated service to marshal types
        /// across the wire
        /// </summary>
        IMediaTypeHandlerRegistry Registry { get; }

        /// <summary>
        /// Adds an interceptor to be notified when a request is sent and a response received
        /// </summary>
        /// <param name="interceptor">The interceptor to add</param>
        /// <returns>A token that automatically unregisters the interceptor when it is disposed</returns>
        IDisposable AddInterceptor(IRequestInterceptor interceptor);

        /// <summary>
        /// Removes an interceptor from the factory
        /// </summary>
        /// <param name="interceptor">The interceptor to remove</param>
        void RemoveInterceptor(IRequestInterceptor interceptor);

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
        TService Create<TService>(Uri baseUri, ICredentials credentials = null) where TService : class;

        /// <summary>
        /// Saves the generated assembly to disk, returning a reference to the assembly as a FileInfo instance
        /// </summary>
        /// <param name="fileName"> The name of the file to save</param>
        FileInfo SaveGeneratedAssembly();
    }
}
