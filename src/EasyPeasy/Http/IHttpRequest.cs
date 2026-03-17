// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IHttpRequest.cs">
//
//  The MIT License (MIT)
//  Copyright © 2013 Matt Channer (mchanner at gmail dot com)
//
//  Permission is hereby granted, free of charge, to any person obtaining a
//  copy of this software and associated documentation files (the "Software"),
//  to deal in the Software without restriction, including without limitation
//  the rights to use, copy, modify, merge, publish, distribute, sublicense,
//  and/or sell copies of the Software, and to permit persons to whom the
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included
//  in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;

namespace EasyPeasy.Http
{
    /// <summary>
    /// Represents an HTTP request abstraction
    /// </summary>
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the request URI
        /// </summary>
        Uri RequestUri { get; }

        /// <summary>
        /// Gets the HTTP method
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the accept header
        /// </summary>
        string Accept { get; set; }

        /// <summary>
        /// Sets a header value
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The header value</param>
        void SetHeader(string name, string value);

        /// <summary>
        /// Gets a header value
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>The header value, or null if not set</returns>
        string GetHeader(string name);

        /// <summary>
        /// Gets the underlying HttpRequestMessage
        /// </summary>
        HttpRequestMessage UnderlyingRequest { get; }
    }
}
