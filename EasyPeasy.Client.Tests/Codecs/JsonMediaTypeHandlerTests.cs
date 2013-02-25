// ------------------------------------------------------------------------------------
// <copyright file="JsonMediaTypeHandlerTests.cs">
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

using System;
using System.IO;

using EasyPeasy.Client.Codecs;
using EasyPeasy.Client.Tests.TestTypes;

using NUnit.Framework;

namespace EasyPeasy.Client.Tests.Codecs
{
    /// <summary>
    /// A set of tests for the <see cref="JsonMediaTypeHandler"/> class.
    /// </summary>
    [TestFixture]
    public class JsonMediaTypeHandlerTests
    {
        /// <summary> The handler under test. </summary>
        private JsonMediaTypeHandler handler;

        /// <summary>
        /// Called before each test is run to set up the test environment
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            handler = new JsonMediaTypeHandler();
        }

        /// <summary>
        /// Tests that an object graph can be written to an output stream
        /// </summary>
        [Test]
        public void Can_write_object_to_output_stream()
        {
            SimpleDto dto = new SimpleDto();

            dto.IntProperty = 10;
            dto.NullableDouble = 23.456;
            dto.StringProperty = "A string";
            dto.Timestamp = DateTime.Now;

            MemoryStream stream = new MemoryStream();

            handler.Produce(dto, stream);

            stream.Seek(0, SeekOrigin.Begin);
            byte[] bytes = stream.ToArray();
            Assert.That(bytes.Length, Is.Not.EqualTo(0));

            string jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            Assert.That(jsonString, Is.Not.Null);
        }

        /// <summary>
        /// Tests that an object can be read from a stream
        /// </summary>
        [Test]
        public void Can_read_object_from_stream()
        {
            string jsonString =
                "{\"StringProperty\":\"A string\",\"IntProperty\":10,\"Timestamp\":\"2013-02-25T08:04:30.4136626+00:00\",\"NullableDouble\":23.456}\"";
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonString);
            MemoryStream stream = new MemoryStream(jsonBytes);

            object result = handler.Consume(stream, typeof(SimpleDto));
            Assert.That(result, Is.Not.Null);

            SimpleDto dto = (SimpleDto)result;
            Assert.That(dto.IntProperty, Is.EqualTo(10));
            Assert.That(dto.StringProperty, Is.EqualTo("A string"));
            Assert.That(dto.StringProperty, Is.EqualTo("A string"));
            Assert.That(dto.NullableDouble.HasValue, Is.True);
            Assert.That(dto.NullableDouble, Is.EqualTo(23.456));
        }
    }
}
