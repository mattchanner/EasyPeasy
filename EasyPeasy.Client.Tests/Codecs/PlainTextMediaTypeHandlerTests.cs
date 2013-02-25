// -----------------------------------------------------------------------
// <copyright file="PlainTextMediaTypeHandlerTests.cs">
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

using System.IO;

using EasyPeasy.Client.Codecs;

using NUnit.Framework;

namespace EasyPeasy.Client.Tests.Codecs
{
    /// <summary>
    /// A set of tests for the <see cref="PlainTextMediaTypeHandler"/> class.
    /// </summary>
    [TestFixture]
    public class PlainTextMediaTypeHandlerTests
    {
        /// <summary> The handler under test. </summary>
        private PlainTextMediaTypeHandler handler;

        /// <summary>
        /// Called before each test is run to set up the test environment
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            handler = new PlainTextMediaTypeHandler();
        }

        /// <summary>
        /// Tests that some plain text can be written to the output stream
        /// </summary>
        [Test]
        public void Can_write_text_to_stream()
        {
            const string SourceString = "easy peasy lemon squeezy";
            MemoryStream stream = new MemoryStream();

            handler.Produce(SourceString, stream);

            stream.Seek(0, SeekOrigin.Begin);

            Assert.That(stream.Length, Is.Not.EqualTo(0));

            byte[] bytes = stream.ToArray();
            string deserialized = System.Text.Encoding.UTF8.GetString(bytes);

            Assert.AreEqual(SourceString, deserialized);
        }

        /// <summary>
        /// Tests that a string can be read form the source stream
        /// </summary>
        [Test]
        public void Can_read_string_from_stream()
        {
            const string SourceString = "easy peasy lemon squeezy";
            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(SourceString));
            stream.Seek(0, SeekOrigin.Begin);

            object result = handler.Consume(stream, typeof(string));
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<string>());
            string stringResult = (string)result;
            Assert.AreEqual(SourceString, stringResult);
        }
    }
}
