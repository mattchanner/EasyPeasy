// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ByteArrayTypeHandlerTests.cs">
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

using System.IO;

using EasyPeasy.Codecs;

using NUnit.Framework;

namespace EasyPeasy.Tests.Codecs
{
    /// <summary>
    /// A set of tests for the <see cref="ByteArrayTypeHandler"/> class.
    /// </summary>
    [TestFixture]
    public class ByteArrayTypeHandlerTests
    {
        /// <summary> The handler under test. </summary>
        private ByteArrayTypeHandler handler;

        /// <summary>
        /// Called before each test is run to set up the test environment
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            handler = new ByteArrayTypeHandler();
        }

        /// <summary>
        /// Asserts that a valid byte array can be written to the output stream
        /// </summary>
        [Test]
        public void Can_write_byte_array()
        {
            const string SourceString = "a test string";
            byte[] source = System.Text.Encoding.UTF8.GetBytes(SourceString);
            MemoryStream body = new MemoryStream();
            handler.WriteObject(null, source, body);
            body.Seek(0, SeekOrigin.Begin);

            Assert.That(body.Length, Is.Not.EqualTo(0));

            string result = System.Text.Encoding.UTF8.GetString(body.ToArray());

            Assert.AreEqual(SourceString, result);
        }

        /// <summary>
        /// Asserts that a valid byte array can be read to the output stream
        /// </summary>
        [Test]
        public void Can_read_byte_array()
        {
            const string SourceString = "a test string";
            byte[] source = System.Text.Encoding.UTF8.GetBytes(SourceString);
            MemoryStream body = new MemoryStream(source);
            body.Seek(0, SeekOrigin.Begin);

            object result = handler.ReadObject(null, body, typeof(byte[]));
            Assert.That(result, Is.Not.Null);

            Assert.That(result, Is.InstanceOf<byte[]>());

            byte[] arrayResult = (byte[])result;
            CollectionAssert.AreEqual(source, arrayResult);
        }
    }
}
