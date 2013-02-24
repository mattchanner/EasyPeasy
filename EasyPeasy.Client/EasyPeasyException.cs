// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EasyPeasyException.cs">
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

namespace EasyPeasy.Client
{
    /// <summary>
    /// A generic exception type for EasyPeasy
    /// </summary>
    [Serializable]
    public class EasyPeasyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPeasyException"/> class.
        /// </summary>
        public EasyPeasyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPeasyException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public EasyPeasyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EasyPeasyException"/> class.
        /// </summary>
        /// <param name="message"> The message. </param>
        /// <param name="innerException"> The inner exception. </param>
        public EasyPeasyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
