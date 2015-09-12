// ------------------------------------------------------------------------------------
// <copyright file="ImageMediaTypeHandlerTests.cs">
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
// ------------------------------------------------------------------------------------

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using EasyPeasy.Codecs;
using EasyPeasy.Tests.Properties;

using NUnit.Framework;

namespace EasyPeasy.Tests.Codecs
{
    /// <summary>
    /// A test fixture for the <see cref="ImageMediaTypeHandler"/> class.
    /// </summary>
    [TestFixture]
    public class ImageMediaTypeHandlerTests
    {
        /// <summary> The handler under test </summary>
        private ImageMediaTypeHandler handler = new ImageMediaTypeHandler(ImageFormat.Png);

        /// <summary>
        /// Tests that the handler can write a valid image to a stream
        /// </summary>
        [Test]
        public void Can_write_and_read_image()
        {
            Image sourceImage = Resources.lemon;

            MemoryStream stream = new MemoryStream();

            handler.WriteObject(null, sourceImage, stream);
            stream.Seek(0, SeekOrigin.Begin);

            Assert.That(stream.Length, Is.Not.EqualTo(0));

            object imageResult = handler.ReadObject(null, stream, typeof(Image));
            Assert.That(imageResult, Is.Not.Null);
            Assert.That(imageResult, Is.InstanceOf<Image>());

            Image deserializedImage = (Image)imageResult;

            Assert.That(sourceImage.Width == deserializedImage.Width);
            Assert.That(sourceImage.Height == deserializedImage.Height);
        }
    }
}
