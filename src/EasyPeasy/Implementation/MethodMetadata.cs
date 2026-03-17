// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodMetadata.cs">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using EasyPeasy.Http;

namespace EasyPeasy.Implementation
{
    /// <summary>
    /// Provides metadata about the service method, the method parameters and their corresponding
    /// attribute information.
    /// </summary>
    public class MethodMetadata
    {
        /// <summary> The content type header. </summary>
        private const string ContentTypeHeader = "content-type";

        /// <summary> The content type header. </summary>
        private const string AcceptsHeader = "accept";

        /// <summary> The query parameters. </summary>
        private ParameterCollection queryParameters;

        /// <summary> The form parameter. </summary>
        private ParameterCollection formParameters;

        /// <summary> The header parameters. </summary>
        private IDictionary<string, object> headerParameters;

        /// <summary> The path parameters. </summary>
        private IDictionary<string, object> pathParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetadata"/> class.
        /// </summary>
        public MethodMetadata()
        {
            this.headerParameters = new Dictionary<string, object>();
            this.pathParameters = new Dictionary<string, object>();
            this.queryParameters = new ParameterCollection();
            this.formParameters = new ParameterCollection();
        }

        /// <summary>
        /// Gets or sets the root path to the service.
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// Gets or sets the path to the service method.
        /// </summary>
        public string MethodPath { get; set; }

        /// <summary>
        /// Gets or sets the media type that the client consumes
        /// </summary>
        public string Consumes { get; set; }

        /// <summary>
        /// Gets or sets the media type that the client produces
        /// </summary>
        public string Produces { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpVerb"/>
        /// </summary>
        public HttpVerb Verb { get; set; }

        /// <summary>
        /// Gets or sets the body to be sent with the request
        /// </summary>
        public object RequestBody { get; set; }

        /// <summary>
        /// Adds a header to the collection
        /// </summary>
        /// <param name="headerName"> The header name. </param>
        /// <param name="headerValue"> The header value. </param>
        public void AddHeaderParameter(string headerName, object headerValue)
        {
            this.headerParameters.Add(headerName, headerValue);
        }

        /// <summary>
        /// Adds a header to the collection
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterValue"> The parameter value. </param>
        public void AddQueryParameter(string parameterName, object parameterValue)
        {
            this.queryParameters.MaybeAdd(parameterName, parameterValue);
        }

        /// <summary>
        /// Adds a header to the collection
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterValue"> The parameter value. </param>
        public void AddFormParameter(string parameterName, object parameterValue)
        {
            this.formParameters.MaybeAdd(parameterName, parameterValue);
        }

        /// <summary>
        /// Adds a path parameter to the collection
        /// </summary>
        /// <param name="parameterName"> The parameter name. </param>
        /// <param name="parameterValue"> The parameter value. </param>
        public void AddPathParameter(string parameterName, object parameterValue)
        {
            this.pathParameters.Add(parameterName, parameterValue);
        }

        /// <summary>
        /// Creates an HTTP request based on the information held within this class
        /// </summary>
        /// <param name="baseUri"> The base Uri. </param>
        /// <param name="credentials"> The credentials. </param>
        /// <param name="mediaRegistry"> The registry of media type handlers</param>
        /// <returns> A tuple containing the HttpRequestMessage and its IHttpRequest wrapper </returns>
        public (HttpRequestMessage, HttpRequestWrapper) CreateRequest(
            Uri baseUri,
            ICredentials credentials,
            IMediaTypeHandlerRegistry mediaRegistry)
        {
            Uri fullUri = this.CreateUri(baseUri);
            HttpMethod method = GetHttpMethod(this.Verb);
            HttpRequestMessage request = new HttpRequestMessage(method, fullUri);
            HttpRequestWrapper wrapper = new HttpRequestWrapper(request);

            // Set Accept header
            if (!string.IsNullOrEmpty(this.Consumes))
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(this.Consumes));
                wrapper.Accept = this.Consumes;
            }

            // Add custom headers
            foreach (var kv in this.headerParameters.Where(kv => kv.Value != null && kv.Key != null))
            {
                if (string.Compare(kv.Key, AcceptsHeader, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(Convert.ToString(kv.Value)));
                }
                else if (string.Compare(kv.Key, ContentTypeHeader, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    request.Headers.TryAddWithoutValidation(kv.Key, Convert.ToString(kv.Value));
                }
            }

            // Choose between form parameters and a body - cannot have both
            if (this.formParameters.Count > 0)
            {
                var formContent = new StringContent(this.formParameters.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
                request.Content = formContent;
                wrapper.ContentType = "application/x-www-form-urlencoded";
            }
            else if (this.RequestBody != null)
            {
                IMediaTypeHandler handler;
                if (mediaRegistry.TryGetHandler(this.RequestBody.GetType(), this.Produces, out handler))
                {
                    var memoryStream = new MemoryStream();
                    handler.WriteObject(wrapper, this.RequestBody, memoryStream);
                    memoryStream.Position = 0;

                    var streamContent = new StreamContent(memoryStream);
                    if (!string.IsNullOrEmpty(this.Produces))
                    {
                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(this.Produces);
                    }
                    request.Content = streamContent;
                    wrapper.ContentType = this.Produces;
                }
            }

            return (request, wrapper);
        }

        /// <summary>
        /// Gets the HttpMethod for the given verb
        /// </summary>
        /// <param name="verb">The HTTP verb</param>
        /// <returns>The HttpMethod</returns>
        private static HttpMethod GetHttpMethod(HttpVerb verb)
        {
            switch (verb)
            {
                case HttpVerb.GET:
                    return HttpMethod.Get;
                case HttpVerb.POST:
                    return HttpMethod.Post;
                case HttpVerb.PUT:
                    return HttpMethod.Put;
                case HttpVerb.DELETE:
                    return HttpMethod.Delete;
                case HttpVerb.HEAD:
                    return HttpMethod.Head;
                case HttpVerb.OPTIONS:
                    return HttpMethod.Options;
                default:
                    return HttpMethod.Get;
            }
        }

        /// <summary>
        /// Constructs a full URI based on a base URI, and the details held within this class.
        /// </summary>
        /// <param name="baseUri">The base URI</param>
        /// <returns>The full URI</returns>
        private Uri CreateUri(Uri baseUri)
        {
            Path endpoint = new Path(this.ServicePath).Append(new Path(this.MethodPath));

            Path specializedPath = endpoint.ReplacePathVariables(this.pathParameters);

            string queryString = this.queryParameters.ToString();

            UriBuilder builder = new UriBuilder(baseUri);

            builder.Path = specializedPath.FullPath;
            builder.Query = queryString;

            return builder.Uri;
        }
    }
}
