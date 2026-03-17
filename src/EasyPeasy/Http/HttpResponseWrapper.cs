// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpResponseWrapper.cs">
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyPeasy.Http
{
    /// <summary>
    /// Wraps an HttpResponseMessage to implement IHttpResponse
    /// </summary>
    internal class HttpResponseWrapper : IHttpResponse
    {
        private readonly HttpResponseMessage response;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpResponseWrapper"/> class.
        /// </summary>
        /// <param name="response">The underlying response</param>
        public HttpResponseWrapper(HttpResponseMessage response)
        {
            this.response = response ?? throw new ArgumentNullException(nameof(response));
        }

        /// <summary>
        /// Gets the HTTP status code
        /// </summary>
        public HttpStatusCode StatusCode => response.StatusCode;

        /// <summary>
        /// Gets the content type
        /// </summary>
        public string ContentType => response.Content?.Headers?.ContentType?.MediaType;

        /// <summary>
        /// Gets the response stream asynchronously
        /// </summary>
        /// <returns>The response stream</returns>
        public Task<Stream> GetResponseStreamAsync()
        {
            return response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Gets the underlying HttpResponseMessage
        /// </summary>
        public HttpResponseMessage UnderlyingResponse => response;
    }
}
