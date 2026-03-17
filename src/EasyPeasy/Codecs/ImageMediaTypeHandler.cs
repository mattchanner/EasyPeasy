// ----------------------------------------------------------------------------
// <copyright file="ImageMediaTypeHandler.cs">
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
// ----------------------------------------------------------------------------

using System;
using System.IO;
using EasyPeasy.Http;
using SkiaSharp;

namespace EasyPeasy.Codecs
{
    /// <summary>
    /// A media type handler for standard image types using SkiaSharp
    /// </summary>
    internal class ImageMediaTypeHandler : IMediaTypeHandler
    {
        /// <summary> The format to use when reading and writing. </summary>
        private readonly SKEncodedImageFormat format;

        /// <summary> The quality level for encoding (1-100). </summary>
        private readonly int quality;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMediaTypeHandler"/> class.
        /// </summary>
        /// <param name="format"> The format to use when reading and writing. </param>
        /// <param name="quality"> The quality level for encoding (1-100). Default is 100. </param>
        public ImageMediaTypeHandler(SKEncodedImageFormat format, int quality = 100)
        {
            this.format = format;
            this.quality = quality;
        }

        /// <summary>
        /// When called, this method is responsible for writing the value to the stream
        /// </summary>
        /// <param name="request">The HTTP request </param>
        /// <param name="value">The value to write</param>
        /// <param name="body">The stream to write to</param>
        public void WriteObject(IHttpRequest request, object value, Stream body)
        {
            if (value is SKBitmap bitmap)
            {
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(format, quality))
                {
                    data.SaveTo(body);
                }
            }
            else if (value is SKImage image)
            {
                using (var data = image.Encode(format, quality))
                {
                    data.SaveTo(body);
                }
            }
            else if (value is byte[] bytes)
            {
                body.Write(bytes, 0, bytes.Length);
            }
            else
            {
                throw new ArgumentException($"Unsupported image type: {value?.GetType()}. Expected SKBitmap, SKImage, or byte[].");
            }
        }

        /// <summary>
        /// When called, this method is responsible for reading the contents of the body stream in order
        /// to generate a response of the type appropriate for the defined media type.
        /// </summary>
        /// <param name="response">The HTTP response </param>
        /// <param name="body"> The stream to write to </param>
        /// <param name="objectType"> The type to de-serialize. </param>
        /// <returns> The <see cref="object"/> read from the stream.  </returns>
        public object ReadObject(IHttpResponse response, Stream body, Type objectType)
        {
            using (var memoryStream = new MemoryStream())
            {
                body.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (var data = SKData.Create(memoryStream))
                {
                    if (objectType == typeof(SKImage))
                    {
                        return SKImage.FromEncodedData(data);
                    }

                    // Default to SKBitmap
                    return SKBitmap.Decode(data);
                }
            }
        }
    }
}
