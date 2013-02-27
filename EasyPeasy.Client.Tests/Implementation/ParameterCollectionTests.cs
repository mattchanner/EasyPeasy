// -----------------------------------------------------------------------
// <copyright file="ParameterCollectionTests.cs">
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

using EasyPeasy.Client.Implementation;

using NUnit.Framework;

namespace EasyPeasy.Client.Tests.Implementation
{
    /// <summary>
    /// A set of unit tests for the <see cref="ParameterCollectionTests"/> class.
    /// </summary>
    [TestFixture]
    public class ParameterCollectionTests
    {
        /// <summary>
        /// The can_add_string_parameter.
        /// </summary>
        [Test]
        public void Can_add_string_parameter()
        {
            string queryString = ParameterCollection.Create().Add("param1", "value1").ToString();
            Assert.That(queryString, Is.EqualTo("param1=value1"));
        }

        /// <summary>
        /// Ensures that two parameters are concatenated with an ampersand
        /// </summary>
        [Test]
        public void Can_add_two_parameters()
        {
            string queryString = ParameterCollection.Create().Add("param1", "value1").Add("param2", "value2").ToString();
            Assert.That(queryString, Is.EqualTo("param1=value1&param2=value2"));
        }

        /// <summary>
        /// Tests that boolean values are serialized as lower case strings
        /// </summary>
        [Test]
        public void Can_add_boolean_parameter()
        {
            string queryString = ParameterCollection.Create().Add("param1", true).Add("param2", false).ToString();
            Assert.That(queryString, Is.EqualTo("param1=true&param2=false"));
        }

        /// <summary>
        /// Passing a null parameter throws an argument null exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_param_name_throws_argument_null_exception()
        {
            ParameterCollection.Create().Add(null, "value");
        }

        /// <summary>
        /// Passing a null parameter throws an argument null exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_param_name_throws_argument_exception()
        {
            ParameterCollection.Create().Add(string.Empty, "value");
        }

        /// <summary>
        /// Passing a null parameter throws an argument null exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_param_name_with_bool_value_throws_argument_null_exception()
        {
            ParameterCollection.Create().Add(null, true);
        }

        /// <summary>
        /// Passing a null parameter throws an argument null exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_param_name_with_bool_value_throws_argument_exception()
        {
            ParameterCollection.Create().Add(string.Empty, true);
        }

        /// <summary>
        /// Empty value throws argument null exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Null_value_throws_argument_null_execption()
        {
            ParameterCollection.Create().Add("param1", null);
        }

        /// <summary>
        /// Empty value throws argument exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Empty_value_throws_argument_execption()
        {
            ParameterCollection.Create().Add("param1", string.Empty);
        }

        /// <summary>
        /// MayBeAdd should not serialize a parameter if the value is null
        /// </summary>
        [Test]
        public void MaybeAdd_does_nothing_with_null_value()
        {
            string queryString = ParameterCollection.Create().MaybeAdd("param", null).ToString();
            Assert.That(queryString, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// MayBeAdd should not serialize a parameter if the value is null
        /// </summary>
        [Test]
        public void MaybeAdd_does_nothing_with_empty_value()
        {
            string queryString = ParameterCollection.Create().MaybeAdd("param", string.Empty).ToString();
            Assert.That(queryString, Is.EqualTo(string.Empty));
        }

        /// <summary>
        /// Ensures that an empty list of query string parameters internally does not cause an empty sequence exception when
        /// calling ToString
        /// </summary>
        [Test]
        public void Empty_query_string_list_returns_empty_string()
        {
            Assert.That(ParameterCollection.Create().ToString(), Is.Empty);
        }

        /// <summary>
        /// Calling AppendTo will append the query string prefixed with a ? character to the
        /// end of the string provided to the method
        /// </summary>
        [Test]
        public void AppendTo_appends_question_mark_to_input_string()
        {
            string fullString = ParameterCollection.Create().Add("p", "1").Add("p2", "2").AppendToPath("http://example.com");
            Assert.That(fullString, Is.EqualTo("http://example.com?p=1&p2=2"));
        }

        /// <summary>
        /// Attempting to add the same key to the query string throws a duplicate key exception
        /// </summary>
        [Test]
        [ExpectedException(typeof(DuplicateKeyException))]
        public void Adding_The_Same_Parameter_Twice_Throws_DuplicateKeyException()
        {
            ParameterCollection.Create().Add("p", true).Add("p", false);
        }
    }
}
