// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FactoryTests.cs">
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
// <summary>
//   A set of tests for the basic functionality of the factory
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

using EasyPeasy.Tests.TestTypes;

using NUnit.Framework;

namespace EasyPeasy.Tests
{
    /// <summary>
    /// A set of tests for the basic functionality of the factory
    /// </summary>
    [TestFixture]
    public class FactoryTests
    {
        /// <summary>
        /// The factory_is_preconfigured_with_handlers.
        /// </summary>
        [Test]
        public void Factory_is_preconfigured_with_handlers()
        {
            EasyPeasyFactory factory = new EasyPeasyFactory(new DefaultMediaTypeRegistry());
            IMediaTypeHandler _;
            Assert.That(factory.Registry.TryGetHandler(typeof(string), MediaType.TextPlain, out _), Is.True);
        }

        /// <summary>
        /// Verifies an interceptor can be registered without error
        /// </summary>
        [Test]
        public void Can_register_request_interceptor()
        {
            EasyPeasyFactory factory = new EasyPeasyFactory(new DefaultMediaTypeRegistry());
            IDisposable token = factory.AddInterceptor(new RecordingInterceptor());
            Assert.That(token, Is.Not.Null);
            token.Dispose();
        }
    }
}
