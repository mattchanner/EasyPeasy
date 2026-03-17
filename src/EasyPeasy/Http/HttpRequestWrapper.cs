// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HttpRequestWrapper.cs">
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
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace EasyPeasy.Http
{
    /// <summary>
    /// Wraps an HttpRequestMessage to implement IHttpRequest
    /// </summary>
    public class HttpRequestWrapper : IHttpRequest
    {
        private readonly HttpRequestMessage request;
        private string contentType;
        private string accept;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestWrapper"/> class.
        /// </summary>
        /// <param name="request">The underlying request</param>
        public HttpRequestWrapper(HttpRequestMessage request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <summary>
        /// Gets the request URI
        /// </summary>
        public Uri RequestUri => request.RequestUri;

        /// <summary>
        /// Gets the HTTP method
        /// </summary>
        public string Method => request.Method.Method;

        /// <summary>
        /// Gets or sets the content type
        /// </summary>
        public string ContentType
        {
            get => contentType;
            set => contentType = value;
        }

        /// <summary>
        /// Gets or sets the accept header
        /// </summary>
        public string Accept
        {
            get => accept;
            set
            {
                accept = value;
                request.Headers.Accept.Clear();
                if (!string.IsNullOrEmpty(value))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(value));
                }
            }
        }

        /// <summary>
        /// Sets a header value
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The header value</param>
        public void SetHeader(string name, string value)
        {
            request.Headers.Remove(name);
            request.Headers.TryAddWithoutValidation(name, value);
        }

        /// <summary>
        /// Gets a header value
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns>The header value, or null if not set</returns>
        public string GetHeader(string name)
        {
            if (request.Headers.TryGetValues(name, out var values))
            {
                return values.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Gets the underlying HttpRequestMessage
        /// </summary>
        public HttpRequestMessage UnderlyingRequest => request;
    }
}
