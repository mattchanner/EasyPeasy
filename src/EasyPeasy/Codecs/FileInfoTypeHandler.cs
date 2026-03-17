// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInfoTypeHandler.cs">
//
//   The MIT License (MIT)
//     Copyright © 2013 Matt Channer (mchanner at gmail dot com)
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the "Software"),
//     to deal in the Software without restriction, including without limitation
//     the rights to use, copy, modify, merge, publish, distribute, sublicense,
//     and/or sell copies of the Software, and to permit persons to whom the
//     Software is furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//     THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EasyPeasy.Http;
using EasyPeasy.Implementation;
using IOPath = System.IO.Path;

namespace EasyPeasy.Codecs
{
    /// <summary>
    /// Represents a <see cref="IMediaTypeHandler"/> for <see cref="FileInfo"/> types.
    /// </summary>
    internal class FileInfoTypeHandler : IMediaTypeHandler
    {
        /// <summary> The header template to use when sending file data </summary>
        private const string HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

        /// <summary> Common MIME type mappings </summary>
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".txt", "text/plain" },
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".css", "text/css" },
            { ".js", "application/javascript" },
            { ".json", "application/json" },
            { ".xml", "application/xml" },
            { ".pdf", "application/pdf" },
            { ".zip", "application/zip" },
            { ".gz", "application/gzip" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },
            { ".tiff", "image/tiff" },
            { ".ico", "image/x-icon" },
            { ".svg", "image/svg+xml" },
            { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" },
            { ".mp4", "video/mp4" },
            { ".avi", "video/x-msvideo" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" }
        };

        /// <summary>
        /// When called, this method is responsible for writing the value to the stream
        /// </summary>
        /// <param name="request">The HTTP request being written to </param>
        /// <param name="value">The value to write</param>
        /// <param name="body">The stream to write to</param>
        public void WriteObject(IHttpRequest request, object value, Stream body)
        {
            Ensure.IsNotNull(value, "value");

            FileInfo file = (FileInfo)value;
            string contentType = GetMimeType(file.Name);

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            body.Write(boundarybytes, 0, boundarybytes.Length);

            // Set the content type with boundary on the underlying request if available
            if (request != null)
            {
                request.ContentType = MediaType.MultipartFormData + "; boundary=" + boundary;
            }

            string header = string.Format(HeaderTemplate, file.Name, file.Name, contentType);
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            body.Write(headerbytes, 0, headerbytes.Length);

            using (FileStream fileStream = file.OpenRead())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    body.Write(buffer, 0, bytesRead);
                }
            }

            byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            body.Write(trailer, 0, trailer.Length);
            body.Flush();
        }

        /// <summary>
        /// When called, this method is responsible for reading the contents of the body stream in order
        /// to generate a response of the type appropriate for the defined media type.
        /// </summary>
        /// <param name="response"> The response being read from. </param>
        /// <param name="body"> The stream to write to </param>
        /// <param name="objectType"> The type to de-serialize.  </param>
        /// <returns> The <see cref="object"/> read from the stream.   </returns>
        public object ReadObject(IHttpResponse response, Stream body, Type objectType)
        {
            throw new NotImplementedException("Reading FileInfo from response is not supported.");
        }

        /// <summary>
        /// Gets the MIME type for a file based on its extension.
        /// </summary>
        /// <param name="fileName"> The filename. </param>
        /// <returns> The MIME type string. </returns>
        protected string GetMimeType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return MediaType.ApplicationOctetStream;
            }

            string fileExtension = IOPath.GetExtension(fileName);

            if (!string.IsNullOrWhiteSpace(fileExtension) && MimeTypes.TryGetValue(fileExtension, out string mimeType))
            {
                return mimeType;
            }

            return MediaType.ApplicationOctetStream;
        }
    }
}
