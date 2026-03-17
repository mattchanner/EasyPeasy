// -----------------------------------------------------------------------
// <copyright file="XmlMediaTypeHandlerTests.cs">
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
// -----------------------------------------------------------------------

using System;
using System.IO;

using EasyPeasy.Codecs;
using EasyPeasy.Tests.TestTypes;

using Xunit;

namespace EasyPeasy.Tests.Codecs
{
    /// <summary>
    /// A set of tests for the <see cref="XmlMediaTypeHandler"/> class.
    /// </summary>
    public class XmlMediaTypeHandlerTests
    {
        /// <summary> The handler under test. </summary>
        private readonly XmlMediaTypeHandler handler;

        public XmlMediaTypeHandlerTests()
        {
            handler = new XmlMediaTypeHandler();
        }

        /// <summary>
        /// Tests that an object graph can be written to an output stream
        /// </summary>
        [Fact]
        public void Can_write_object_to_output_stream()
        {
            SimpleDto dto = new SimpleDto();

            dto.IntProperty = 10;
            dto.NullableDouble = 23.456;
            dto.StringProperty = "A string";
            dto.Timestamp = DateTime.Now;

            MemoryStream stream = new MemoryStream();

            handler.WriteObject(null, dto, stream);

            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = stream.ToArray();
            Assert.NotEqual(0, bytes.Length);

            string xmlString = System.Text.Encoding.UTF8.GetString(bytes);
            Assert.NotNull(xmlString);
        }

        /// <summary>
        /// Tests that an object can be read from a stream
        /// </summary>
        [Fact]
        public void Can_read_object_from_stream()
        {
            SimpleDto dto = new SimpleDto();

            dto.IntProperty = 10;
            dto.NullableDouble = 23.456;
            dto.StringProperty = "A string";
            dto.Timestamp = DateTime.Now;

            MemoryStream stream = new MemoryStream();

            handler.WriteObject(null, dto, stream);

            stream.Seek(0, SeekOrigin.Begin);

            object result = handler.ReadObject(null, stream, typeof(SimpleDto));
            Assert.NotNull(result);

            dto = (SimpleDto)result;
            Assert.Equal(10, dto.IntProperty);
            Assert.Equal("A string", dto.StringProperty);
            Assert.True(dto.NullableDouble.HasValue);
            Assert.Equal(23.456, dto.NullableDouble);
        }
    }
}
