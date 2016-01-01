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
using System.Reflection;

using EasyPeasy.Implementation;
using EasyPeasy.Tests.TestTypes.Reflected;

using NUnit.Framework;

namespace EasyPeasy.Tests.Implementation
{
    /// <summary>
    /// Tests for the ReflectionUtils class
    /// </summary>
    [TestFixture]
    public class ReflectionUtilsTests
    {
        /// <summary>
        /// If a method does not specify a verb attribute (GET, PUT, POST, DELETE), then GET is 
        /// returned
        /// </summary>
        [Test]
        public void Defaults_to_GET_verb_when_method_does_not_specify()
        {
            MethodInfo getDataMethod = GetDataMethod(TestServiceInterface());

            HttpVerb method = ReflectionUtils.DetermineHttpVerb(getDataMethod);

            Assert.AreEqual(HttpVerb.GET, method);
        }

        /// <summary>
        /// Verifies the presence of a verb attribute on a method will be used instead
        /// of the default GET
        /// </summary>
        [Test]
        public void Uses_the_verb_attribute_when_present()
        {
            MethodInfo updateMethod = UpdateMethod(TestServiceInterface());

            HttpVerb method = ReflectionUtils.DetermineHttpVerb(updateMethod);

            Assert.AreEqual(HttpVerb.POST, method);
        }

        /// <summary>
        /// If the method is not annotated with a Consumes attribute, the interface default
        /// is used instead
        /// </summary>
        [Test]
        public void Consumes_media_type_is_taken_from_interface_when_not_defined_on_method()
        {
            MethodInfo getDataMethod = GetDataMethod(TestServiceInterface());

            string mediaType = ReflectionUtils.DetermineConsumesMediaType(getDataMethod, string.Empty);

            Assert.AreEqual(MediaType.ApplicationJson, mediaType);
        }

        /// <summary>
        /// If the method is annotated with a Consumes attribute, this is used instead of
        /// the interface one
        /// </summary>
        [Test]
        public void Consumes_media_type_is_taken_from_method_when_it_is_present()
        {
            MethodInfo updateMethod = UpdateMethod(TestServiceInterface());

            string mediaType = ReflectionUtils.DetermineConsumesMediaType(updateMethod, string.Empty);

            Assert.AreEqual(MediaType.TextXml, mediaType);
        }

        /// <summary>
        /// Returns the Type representation for the ITestService1 interface
        /// </summary>
        /// <returns>The type representation</returns>
        private Type TestServiceInterface()
        {
            return typeof(ITestService1);
        }

        private Type TestService2Interface()
        {
            return typeof(ITestService2);
        }

        /// <summary>
        /// Returns the MethodInfo representation of the GetData method
        /// </summary>
        /// <param name="serviceType">The service interface type</param>
        /// <returns>The method reference</returns>
        private MethodInfo GetDataMethod(Type serviceType)
        {
            return serviceType.GetMethod("GetData");
        }

        /// <summary>
        /// Returns the MethodInfo representation of the Update method
        /// </summary>
        /// <param name="serviceType">The service interface type</param>
        /// <returns>The method reference</returns>
        private MethodInfo UpdateMethod(Type serviceType)
        {
            return serviceType.GetMethod("Update");
        }
    }
}
