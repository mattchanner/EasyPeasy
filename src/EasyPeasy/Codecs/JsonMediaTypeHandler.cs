﻿// -----------------------------------------------------------------------
// <copyright file="JsonMediaTypeHandler.cs">
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
// -----------------------------------------------------------------------

using System;
using System.IO;
using System.Net;

using Newtonsoft.Json;

namespace EasyPeasy.Codecs
{
    /// <summary>
    /// A JSON handler
    /// </summary>
    internal class JsonMediaTypeHandler : IMediaTypeHandler
    {
        /// <summary>
        /// When called, this method is responsible for writing the value to the stream
        /// </summary>
        /// <param name="request">The web request </param>
        /// <param name="value">The value to write</param>
        /// <param name="body">The stream to write to</param>
        public void WriteObject(WebRequest request, object value, Stream body)
        {
            JsonSerializer serializer = new JsonSerializer();
            TextWriter textWriter = new StreamWriter(body);
            serializer.Serialize(textWriter, value);
            textWriter.Flush();
        }

        /// <summary>
        /// When called, this method is responsible for reading the contents of the body stream in order
        /// to generate a response of the type appropriate for the defined media type.
        /// </summary>
        /// <param name="response">The web response </param>
        /// <param name="body"> The stream to write to </param>
        /// <param name="objectType"> The type to de-serialize. </param>
        /// <returns> The <see cref="object"/> read from the stream.  </returns>
        public object ReadObject(WebResponse response, Stream body, Type objectType)
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize(new StreamReader(body), objectType);
        }
    }
}
