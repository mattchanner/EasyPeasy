// -----------------------------------------------------------------------
// <copyright file="MediaType.cs">
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
// ----------------------------------------------------------------------------

namespace EasyPeasy
{
    /// <summary>
    /// Represents various common media types
    /// </summary>
    public static class MediaType
    {
        /// <summary> Plain text media type </summary>
        public const string TextPlain = "text/plain";

        /// <summary> HTML text media type </summary>
        public const string TextHtml = "text/html";

        /// <summary> XML text media type </summary>
        public const string TextXml = "text/xml";

        /// <summary> Application XML media type </summary>
        public const string ApplicationXml = "application/xml";

        /// <summary> Application JSON media type </summary>
        public const string ApplicationJson = "application/json";

        /// <summary> PNG Image media type </summary>
        public const string ImagePNG = "image/png";

        /// <summary> GIF Image media type </summary>
        public const string ImageGIF = "image/gif";

        /// <summary> JPEG Image media type </summary>
        public const string ImageJPG = "image/jpeg";

        /// <summary> TIFF Image media type </summary>
        public const string ImageTIFF = "image/tiff";

        /// <summary> Bitmap Image media type </summary>
        public const string ImageBMP = "image/bmp";
    }
}
