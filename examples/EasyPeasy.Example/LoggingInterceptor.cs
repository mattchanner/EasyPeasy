// -----------------------------------------------------------------------------
// <copyright file="IContactService.cs">
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
// ------------------------------------------------------------------------------
using System;
using System.Net;

namespace EasyPeasy
{
    /// <summary>
    /// This is a simple example of an interceptor to log out to the console each of the events
    /// raised by EasyPeasy when a request is made.
    /// </summary>
    class LoggingInterceptor : IRequestInterceptor
    {
        public void OnBeforeSend(WebRequest webRequest)
        {
            Console.WriteLine("[OnBeforeSend] URL: {0}", webRequest.RequestUri);
        }

        public void OnError(WebException exception)
        {
            Console.Error.WriteLine("[OnError] {0}", exception);
        }

        public void OnReceive(WebResponse response)
        {
            Console.WriteLine("[OnReceive] Content Type {0}, URL: {1}", response.ContentType, response.ResponseUri);
        }
    }
}
