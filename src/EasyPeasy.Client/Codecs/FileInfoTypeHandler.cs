// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileInfoTypeHandler.cs">
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

using System;
using System.IO;
using System.Net;
using System.Text;

using EasyPeasy.Client.Implementation;

using Microsoft.Win32;

using Path = System.IO.Path;

namespace EasyPeasy.Client.Codecs
{
    /// <summary>
    /// Represents a <see cref="IMediaTypeHandler"/> for <see cref="FileInfo"/> types.
    /// </summary>
    internal class FileInfoTypeHandler : IMediaTypeHandler
    {
        /// <summary> The template to use for each form item sent when uploading a file </summary>
        private const string FormdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";

        /// <summary> The header template to use when sending file data </summary>
        private const string HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

        /// <summary>
        /// When called, this method is responsible for writing the value to the stream
        /// </summary>
        /// <param name="request">The web request being written to </param>
        /// <param name="value">The value to write</param>
        /// <param name="body">The stream to write to</param>
        public void WriteObject(WebRequest request, object value, Stream body)
        {
            Ensure.IsNotNull(request, "request");
            Ensure.IsNotNull(value, "value");

            FileInfo file = (FileInfo)value;
            string contentType = GetMimeFromRegistry(file.Name);

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            // TODO: form parameters should be considered at some point
            body.Write(boundarybytes, 0, boundarybytes.Length);

            HttpWebRequest wr = (HttpWebRequest)request;

            wr.ContentType = MediaType.MultipartFormData + "; boundary=" + boundary;
            wr.KeepAlive = true;
            
            string header = string.Format(HeaderTemplate, file.Name, file.Name, contentType);
            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            body.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = file.OpenRead();
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                body.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();

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
        public object ReadObject(WebResponse response, Stream body, Type objectType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The get mime from registry.
        /// </summary>
        /// <param name="fileName"> The filename. </param>
        /// <returns> The <see cref="string"/> containing the mime type as found in the windows registry. </returns>
        protected string GetMimeFromRegistry(string fileName)
        {
            string mime = MediaType.ApplicationOctetStream;

            if (!string.IsNullOrEmpty(fileName))
            {
                string fileExtension = Path.GetExtension(fileName);

                if (!string.IsNullOrWhiteSpace(fileExtension))
                {
                    RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey(fileExtension);
                    if (registryKey != null && registryKey.GetValue("Content Type") != null)
                        mime = registryKey.GetValue("Content Type").ToString();
                }
            }

            return mime;
        }
    }
}
