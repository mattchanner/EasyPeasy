// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IRequestInterceptor.cs">
//
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

namespace EasyPeasy
{
    /// <summary>
    /// Represents a filter that can be added to the request pipeline
    /// </summary>
    public interface IRequestInterceptor
    {
        /// <summary>
        /// Called before a request is made to the server
        /// </summary>
        /// <param name="webRequest"> The request being sent. </param>
        void OnBeforeSend(WebRequest webRequest);

        /// <summary>
        /// Called once a response has been received from the server
        /// </summary>
        /// <param name="response">The received response</param>
        void OnReceive(WebResponse response);

        /// <summary>
        /// Receives notifications about an error
        /// </summary>
        /// <param name="exception"> The exception. </param>
        void OnError(WebException exception);
    }
}
