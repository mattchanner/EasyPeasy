// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RecordingInterceptor.cs">
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

using System.Net;

namespace EasyPeasy.Client.Tests.TestTypes
{
    /// <summary>
    /// Interceptor used for testing
    /// </summary>
    public class RecordingInterceptor : IRequestInterceptor
    {
        /// <summary> The request. </summary>
        private WebRequest request;

        /// <summary> The response. </summary>
        private WebResponse response;

        /// <summary> The exception. </summary>
        private WebException exception;

        /// <summary>
        /// Gets a value indicating whether the interceptor received a request.
        /// </summary>
        public bool HasRequest
        {
            get
            {
                return request != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the interceptor received a response.
        /// </summary>
        public bool HasResponse
        {
            get
            {
                return response != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the interceptor received an exception.
        /// </summary>
        public bool HasException
        {
            get
            {
                return exception != null;
            }
        }

        /// <summary>
        /// Called before a request is made to the server
        /// </summary>
        /// <param name="webRequest"> The request being sent. </param>
        public void OnBeforeSend(WebRequest webRequest)
        {
            this.request = webRequest;
        }

        /// <summary>
        /// Called once a response has been received from the server
        /// </summary>
        /// <param name="response">The received response</param>
        public void OnReceive(WebResponse response)
        {
            this.response = response;
        }

        /// <summary>
        /// Receives notifications about an error
        /// </summary>
        /// <param name="exception"> The exception. </param>
        public void OnError(WebException exception)
        {
            this.exception = exception;
        }
    }
}
