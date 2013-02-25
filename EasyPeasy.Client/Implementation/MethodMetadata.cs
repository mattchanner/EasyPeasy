// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodMetadata.cs">
//
//  The MIT License (MIT)
//  Copyright © 2013 Matt Channer (mchanner at gmail dot com)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a 
//  copy of this software and associated documentation files (the “Software”),
//  to deal in the Software without restriction, including without limitation 
//  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//  and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included 
//  in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS 
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
using System.Linq;
using System.Net;

namespace EasyPeasy.Client.Implementation
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodMetadata"/> class.
        /// </summary>
        public MethodMetadata()
        {
            this.Headers = new Dictionary<string, object>();
            this.QueryParameters = new SortedDictionary<string, object>();
            this.PathParameters = new SortedDictionary<string, object>();
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
        /// Gets or sets the media type that the service consumes
        /// </summary>
        public string Consumes { get; set; }

        /// <summary>
        /// Gets or sets the media type that the service produces
        /// </summary>
        public string Produces { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpVerb"/>
        /// </summary>
        public HttpVerb Verb { get; set; }

        /// <summary>
        /// Gets the custom headers to add to the request.
        /// </summary>
        public IDictionary<string, object> Headers { get; private set; }

        /// <summary>
        /// Gets the query string parameters to add to the request
        /// </summary>
        public IDictionary<string, object> QueryParameters { get; private set; }

        /// <summary>
        /// Gets the path parameters to add to the request
        /// </summary>
        public IDictionary<string, object> PathParameters { get; private set; }

        /// <summary>
        /// Gets or sets the body to be sent with the request
        /// </summary>
        public object RequestBody { get; set; }

        /// <summary>
        /// Creates a web request based on the information held within this class
        /// </summary>
        /// <param name="baseUri"> The base Uri. </param>
        /// <param name="credentials"> The credentials. </param>
        /// <param name="mediaRegistry"> The registry of media type handlers</param>
        /// <returns> The created request </returns>
        public WebRequest CreateRequest(
            Uri baseUri, 
            ICredentials credentials, 
            IMediaTypeHandlerRegistry mediaRegistry)
        {
            Uri fullUri = this.CreateUri(baseUri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullUri);
            
            request.Credentials = credentials;
            request.Method = this.Verb.ToString();
            request.ContentType  = this.Consumes;
            request.Accept = this.Produces;

            foreach (var kv in this.Headers.Where(kv => kv.Value != null && kv.Key != null))
            {
                // WebRequest throws exceptions if the Accept and Content-Type headers are set indirectly,
                // so need to check for these explicitly and set the associated properties if found
                if (string.Compare(kv.Key, AcceptsHeader, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    request.Accept = Convert.ToString(kv.Value);
                }
                else if (string.Compare(kv.Key, ContentTypeHeader, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    request.ContentType = Convert.ToString(kv.Value);
                }
                else
                {
                    request.Headers.Add(kv.Key, Convert.ToString(kv.Value));    
                }
            }

            if (this.RequestBody != null)
            {
                IMediaTypeHandler handler;
                if (mediaRegistry.TryGetHandler(this.RequestBody.GetType(), this.Consumes, out handler))
                {
                    handler.WriteObject(request, this.RequestBody, request.GetRequestStream());
                }
            }

            return request;
        }

        /// <summary>
        /// Constructs a full URI based on a base URI, and the details held within this class.
        /// </summary>
        /// <param name="baseUri">The base URI</param>
        /// <returns>The full URI</returns>
        private Uri CreateUri(Uri baseUri)
        {
            Path endpoint = new Path(this.ServicePath).Append(new Path(this.MethodPath));

            Path specializedPath = endpoint.ReplacePathVariables(this.PathParameters);

            QueryString qs =
                QueryString.Create().AddAll(
                    this.QueryParameters
                        .Where(kv => kv.Key != null && kv.Value != null)
                        .Select(kv => Tuple.Create(kv.Key, kv.Value.ToString())));

            UriBuilder builder = new UriBuilder(baseUri);

            builder.Path = specializedPath.FullPath;
            builder.Query = qs.ToString();

            return builder.Uri;
        }
    }
}
