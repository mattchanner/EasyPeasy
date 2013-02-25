// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceClient.cs">
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
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// The base class for each client service
    /// </summary>
    public abstract class ServiceClient : IServiceClient
    {
        /// <summary> The default amount of time to wait before timing out </summary>
        private const int DefaultTimeoutMs = 1000 * 32;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClient"/> class.
        /// </summary>
        protected ServiceClient()
        {
            this.Timeout = TimeSpan.FromMilliseconds(DefaultTimeoutMs);
        }

        /// <summary>
        /// Gets or sets the amount of time to wait for a synchronous request before timing out
        /// </summary>
        public TimeSpan Timeout { get; set; }

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
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <typeparam name="T"> The type to be returned by the service call </typeparam>
        /// <returns> The <see cref="Task"/> which when run to completion, returns the result of 
        /// calling the service. </returns>
        protected Task<T> AsyncRequestWithResult<T>(MethodMetadata methodProperties)
        {
            IMediaTypeHandler handler;
            if (!this.MediaRegistry.TryGetHandler(typeof(T), methodProperties.Produces, out handler))
                throw new EasyPeasyException(methodProperties.Produces + " does not have a valid handler");
            
            Task<WebResponse> task = CreateRequest(methodProperties);

            return task.ContinueWith(t =>
                {
                    CheckTaskForException(t);
                    return (T)handler.ReadObject(t.Result, t.Result.GetResponseStream(), typeof(T));
                });
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <returns> The raw web response. </returns>
        protected Task<WebResponse> AsyncRequestWithRawResponse(MethodMetadata methodProperties)
        {
            return CreateRequest(methodProperties);
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <returns> The <see cref="Task"/> which when run to completion, returns the result of 
        /// calling the service. </returns>
        protected Task AsyncVoidRequest(MethodMetadata methodProperties)
        {
            return CreateRequest(methodProperties);
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <typeparam name="T"> The type to be returned by the service call </typeparam>
        /// <returns> The result of calling the service. </returns>
        protected T SyncRequestWithResult<T>(MethodMetadata methodProperties)
        {
            IMediaTypeHandler handler;
            if (!this.MediaRegistry.TryGetHandler(typeof(T), methodProperties.Produces, out handler))
                throw new EasyPeasyException(methodProperties.Produces + " does not have a valid handler");
            
            WebResponse response = SyncRequestWithRawResponse(methodProperties);
            return (T)handler.ReadObject(response, response.GetResponseStream(), typeof(T));
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        /// <returns> The raw web response. </returns>
        protected WebResponse SyncRequestWithRawResponse(MethodMetadata methodProperties)
        {
            Task<WebResponse> task = CreateRequest(methodProperties);
            task.Wait(Timeout);
            CheckTaskForException(task);

            return task.Result;
        }

        /// <summary>
        /// Executes a service request based on metadata provided by the given <see cref="MethodInfo"/>, and supplied
        /// runtime arguments.
        /// </summary>
        /// <param name="methodProperties"> The details about the method to invoke. </param>
        protected void SyncVoidRequest(MethodMetadata methodProperties)
        {
            Task<WebResponse> task = CreateRequest(methodProperties);
            task.Wait(Timeout);
            CheckTaskForException(task);
        }

        /// <summary>
        /// Checks the status of the task and if it is in a faulted state, will throw the exception.
        /// </summary>
        /// <typeparam name="T">The task type</typeparam>
        /// <param name="task">The task to check</param>
        private void CheckTaskForException<T>(Task<T> task)
        {
            if (task.Exception != null)
            {
                throw task.Exception.Flatten();
            }
        }

        /// <summary>
        /// Creates a new web request and returns the result as a task.
        /// </summary>
        /// <param name="methodProperties"> The method properties. </param>
        /// <returns> The created <see cref="Task"/>. </returns>
        private Task<WebResponse> CreateRequest(MethodMetadata methodProperties)
        {
            WebRequest request = methodProperties.CreateRequest(this.BaseUri, this.Credentials, this.MediaRegistry);
            return Task<WebResponse>.Factory.FromAsync(request.BeginGetResponse, request.EndGetResponse, null);
        }
    }
}
