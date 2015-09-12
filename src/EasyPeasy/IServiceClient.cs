// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IServiceClient.cs">
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
using System.Net;

namespace EasyPeasy
{
    /// <summary>
    /// Represents the client interface that all proxy services will implement in addition to the one requested
    /// by the caller.
    /// </summary>
    public interface IServiceClient
    {
        /// <summary> Raised once a request is constructed, and before it is sent. </summary>
        event EventHandler<WebRequestEventArgs> BeforeSend;

        /// <summary> Raised once a response has been received. </summary>
        event EventHandler<WebResponseEventArgs> ResponseReceived;

        /// <summary> Raised when an exception is returned by the server </summary>
        event EventHandler<WebExceptionEventArgs> ExceptionReceived;

        /// <summary>
        /// Gets or sets the base URI to use for each service method
        /// </summary>
        Uri BaseUri { get; set; }

        /// <summary>
        /// Gets or sets the amount of time to wait on a synchronous request before timing out
        /// </summary>
        TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the credentials to be sent with each service request
        /// </summary>
        ICredentials Credentials { get; set; }

        /// <summary>
        /// Gets or sets the registry to use for serializing types
        /// </summary>
        IMediaTypeHandlerRegistry MediaRegistry { get; set; }
    }
}
