// ---------------------------------------------------------------------------------------------------
// <copyright file="ServiceClient.cs">
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
// ---------------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EasyPeasy.Http;

namespace EasyPeasy.Implementation
{
    /// <summary>
    /// The base class for each client service
    /// </summary>
    public abstract class ServiceClient : IServiceClient
    {
        /// <summary> The default amount of time to wait before timing out </summary>
        private const int DefaultTimeoutMs = 1000 * 32;

        /// <summary> The HTTP client instance </summary>
        private readonly HttpClient httpClient;

        /// <summary> Whether we own the HttpClient and should dispose it </summary>
        private readonly bool ownsHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClient"/> class.
        /// </summary>
        protected ServiceClient() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClient"/> class.
        /// </summary>
        /// <param name="httpClient">Optional HttpClient to use. If null, a new one will be created.</param>
        protected ServiceClient(HttpClient httpClient)
        {
            if (httpClient == null)
            {
                this.httpClient = new HttpClient();
                this.ownsHttpClient = true;
            }
            else
            {
                this.httpClient = httpClient;
                this.ownsHttpClient = false;
            }

            this.Timeout = TimeSpan.FromMilliseconds(DefaultTimeoutMs);
        }

        /// <summary>
        /// Raised before the request is sent
        /// </summary>
        public event EventHandler<HttpRequestEventArgs> BeforeSend;

        /// <summary>
        /// Raised after a response is received
        /// </summary>
        public event EventHandler<HttpResponseEventArgs> ResponseReceived;

        /// <summary>
        /// Raised when an exception is received
        /// </summary>
        public event EventHandler<HttpExceptionEventArgs> ExceptionReceived;

        /// <summary>
        /// Gets or sets the amount of time to wait for a request before timing out
        /// </summary>
        public TimeSpan Timeout
        {
            get => httpClient.Timeout;
            set => httpClient.Timeout = value;
        }

        /// <summary>
        /// Gets or sets the base URI to use for each service method
        /// </summary>
        public Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the credentials to be sent with each service request
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the registry to use for serializing types
        /// </summary>
        public IMediaTypeHandlerRegistry MediaRegistry { get; set; }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodMetadata"/>,
        /// and supplied runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <typeparam name="T"> The type to be returned by the service call </typeparam>
        /// <returns> The <see cref="Task"/> which when run to completion, returns the result of
        /// calling the service. </returns>
        protected async Task<T> AsyncRequestWithResult<T>(MethodMetadata methodProperties)
        {
            IMediaTypeHandler handler;
            if (!this.MediaRegistry.TryGetHandler(typeof(T), methodProperties.Consumes, out handler))
                throw new EasyPeasyException(methodProperties.Produces + " does not have a valid handler");

            var (request, response) = await SendRequestAsync(methodProperties).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
                var wrappedResponse = new HttpResponseWrapper(response);
                this.OnResponseReceived(new HttpResponseEventArgs(wrappedResponse));

                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return (T)handler.ReadObject(wrappedResponse, stream, typeof(T));
            }
            catch (HttpRequestException ex)
            {
                this.OnExceptionReceived(new HttpExceptionEventArgs(ex));
                throw;
            }
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodMetadata"/>,
        /// and supplied runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <returns> The raw HTTP response. </returns>
        protected async Task<IHttpResponse> AsyncRequestWithRawResponse(MethodMetadata methodProperties)
        {
            var (request, response) = await SendRequestAsync(methodProperties).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
                var wrappedResponse = new HttpResponseWrapper(response);
                this.OnResponseReceived(new HttpResponseEventArgs(wrappedResponse));
                return wrappedResponse;
            }
            catch (HttpRequestException ex)
            {
                this.OnExceptionReceived(new HttpExceptionEventArgs(ex));
                throw;
            }
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodMetadata"/>,
        /// and supplied runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <returns> The <see cref="Task"/> which when run to completion, returns the result of
        /// calling the service. </returns>
        protected async Task AsyncVoidRequest(MethodMetadata methodProperties)
        {
            var (request, response) = await SendRequestAsync(methodProperties).ConfigureAwait(false);

            try
            {
                response.EnsureSuccessStatusCode();
                var wrappedResponse = new HttpResponseWrapper(response);
                this.OnResponseReceived(new HttpResponseEventArgs(wrappedResponse));
            }
            catch (HttpRequestException ex)
            {
                this.OnExceptionReceived(new HttpExceptionEventArgs(ex));
                throw;
            }
        }

        /// <summary>
        /// Raises the <see cref="IServiceClient.BeforeSend"/> event.
        /// </summary>
        /// <param name="args"> The event arguments. </param>
        protected virtual void OnBeforeSend(HttpRequestEventArgs args)
        {
            var evt = BeforeSend;
            if (evt != null)
            {
                evt(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="IServiceClient.ResponseReceived"/> event.
        /// </summary>
        /// <param name="args"> The event arguments. </param>
        protected virtual void OnResponseReceived(HttpResponseEventArgs args)
        {
            var evt = ResponseReceived;
            if (evt != null)
            {
                evt(this, args);
            }
        }

        /// <summary>
        /// Raises the <see cref="IServiceClient.ExceptionReceived"/> event.
        /// </summary>
        /// <param name="args"> The event arguments. </param>
        protected virtual void OnExceptionReceived(HttpExceptionEventArgs args)
        {
            var evt = ExceptionReceived;
            if (evt != null)
            {
                evt(this, args);
            }
        }

        /// <summary>
        /// Creates and sends an HTTP request.
        /// </summary>
        /// <param name="methodProperties"> The method properties. </param>
        /// <returns> A tuple containing the request wrapper and response. </returns>
        private async Task<(HttpRequestWrapper request, HttpResponseMessage response)> SendRequestAsync(MethodMetadata methodProperties)
        {
            var (requestMessage, wrappedRequest) = methodProperties.CreateRequest(
                this.BaseUri, this.Credentials, this.MediaRegistry);

            // Raise event to callers that the request has been created
            this.OnBeforeSend(new HttpRequestEventArgs(wrappedRequest));

            var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return (wrappedRequest, response);
        }
    }
}
