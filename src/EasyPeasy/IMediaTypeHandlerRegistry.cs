﻿// ----------------------------------------------------------------------------------
// <copyright file="IMediaTypeHandlerRegistry.cs">
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
// -----------------------------------------------------------------------------------

using System;

namespace EasyPeasy
{
    /// <summary>
    /// The registry used by each service for accessing handlers for serializing and de-serializing types
    /// </summary>
    public interface IMediaTypeHandlerRegistry
    {
        /// <summary>
        /// Registers a handler for a given media type.
        /// </summary>
        /// <param name="mediaType">The media type to register against</param>
        /// <param name="handler">The handler to register</param>
        void RegisterMediaTypeHandler(string mediaType, IMediaTypeHandler handler);

        /// <summary>
        /// Registers an override to be applied for a specific <see cref="Type"/> regardless of what
        /// the service states it requires.
        /// </summary>
        /// <param name="type">The type to register a custom handler for</param>
        /// <param name="handler">The handler to use</param>
        void RegisterCustomTypeHandler(Type type, IMediaTypeHandler handler);

        /// <summary>
        /// Attempts to locate a <see cref="IMediaTypeHandler"/> that can handle the requested type.
        /// If a custom handler is available for the supplied type, this will be used in preference to
        /// the media type. The method returns false if no handler is found that matches either criteria.
        /// </summary>
        /// <param name="objectType">The type of object to read or write</param>
        /// <param name="mediaType">The requested media type by the service</param>
        /// <param name="handler">The returned handler if a suitable one is found</param>
        /// <returns>True if an appropriate handler was found, otherwise false</returns>
        bool TryGetHandler(Type objectType, string mediaType, out IMediaTypeHandler handler);
    }
}
