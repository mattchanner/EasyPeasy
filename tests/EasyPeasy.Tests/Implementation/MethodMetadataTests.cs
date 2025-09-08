// -----------------------------------------------------------------------
// <copyright file="MethodMetadataTests.cs">
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

using EasyPeasy.Implementation;

using NUnit.Framework;
using System;
using System.IO;
using System.Net;

namespace EasyPeasy.Tests.Implementation
{
    /// <summary>
    /// A set of tests for the <see cref="MethodMetadata"/> class.
    /// </summary>
    [TestFixture]
    public class MethodMetadataTests
    {
        /// <summary> The credentials to use when creating the request </summary>
        private readonly ICredentials Credentials = new NetworkCredential("username", "password");

        /// <summary> The URI to use when creating the request </summary>
        private readonly Uri TestUri = new Uri("http://example.com");

        /// <summary>
        /// The [Consumes] attribute is typically used for setting the Accept
        /// header on the request
        /// </summary>
        [Test]
        public void Accept_header_defaults_to_consumes_property()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.Consumes = "application/json";

            HttpWebRequest request = (HttpWebRequest)CreateRequest(metadata);

            Assert.Equals("application/json", request.Accept);
        }

        /// <summary>
        /// The [Verb] attribute sets the HTTP method to use
        /// </summary>
        [Test]
        public void Request_method_is_set_to_verb_property()
        {
            MethodMetadata metadata = new MethodMetadata { Verb = HttpVerb.DELETE };

            HttpWebRequest request = (HttpWebRequest)CreateRequest(metadata);

            Assert.Equals("DELETE", request.Method);
        }

        /// <summary>
        /// The [Verb] attribute sets the HTTP method to use
        /// </summary>
        [Test]
        public void Uri_is_set_on_request()
        {
            MethodMetadata metadata = new MethodMetadata();

            HttpWebRequest request = (HttpWebRequest)CreateRequest(metadata);

            Assert.Equals(TestUri, request.RequestUri);
        }

        /// <summary>
        /// Verifies the credentials passed in to the CreateRequest method are
        /// set correctly on the Request instance
        /// </summary>
        [Test]
        public void Credentials_are_set_on_request()
        {
            MethodMetadata metadata = new MethodMetadata();

            WebRequest request = CreateRequest(metadata);

            Assert.Equals(Credentials, request.Credentials);
        }

        [Test]
        public void Can_add_header_parameter()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.AddHeaderParameter("Header-Name", "Header-Value");

            WebRequest request = CreateRequest(metadata);

            Assert.Equals("Header-Value", request.Headers["Header-Name"]);
        }

        /// <summary>
        /// Content type header needs to be special cased (not added to the headers
        /// collection directly)
        /// </summary>
        [Test]
        public void Can_add_content_type_header()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.AddHeaderParameter("Content-Type", "application/json");

            WebRequest request = CreateRequest(metadata);

            Assert.Equals("application/json", request.ContentType);
        }

        /// <summary>
        /// Accept header needs to be special cased (not added to the headers
        /// collection directly)
        /// </summary>
        [Test]
        public void Can_add_accept_header()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.AddHeaderParameter("Accept", "application/json");

            HttpWebRequest request = (HttpWebRequest)CreateRequest(metadata);

            Assert.Equals("application/json", request.Accept);
        }

        /// <summary>
        /// Tests that the request URI is comprised of both the base Uri and the service path
        /// </summary>
        [Test]
        public void Full_uri_uses_service_path()
        {
            MethodMetadata meta = new MethodMetadata { ServicePath = "/service/1.0" };

            WebRequest request = CreateRequest(meta);

            Assert.Equals("http://example.com/service/1.0/", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Tests that the Path attribute added to the method is included in the URI
        /// </summary>
        [Test]
        public void Method_path_is_appended_to_uri()
        {
            MethodMetadata meta = new MethodMetadata
            {
                ServicePath = "api/v1",
                MethodPath = "/action"
            };

            WebRequest request = CreateRequest(meta);

            Assert.Equals("http://example.com/api/v1/action", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Verifes that templated parts of a URI can be substituted with path parameters
        /// </summary>
        [Test]
        public void Uri_can_be_substituted_with_path_params()
        {
            MethodMetadata meta = new MethodMetadata
            {
                ServicePath = "services/{apiVersion}",
                MethodPath = "/users/{user}"
            };

            meta.AddPathParameter("apiVersion", "1.0");
            meta.AddPathParameter("user", "matt");

            WebRequest request = CreateRequest(meta);

            Assert.Equals("http://example.com/services/1.0/users/matt", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Verifies that values provided in path parameters are escaped correctly
        /// </summary>
        [Test]
        public void Uri_is_escaped()
        {
            MethodMetadata meta = new MethodMetadata
            {
                MethodPath = "/users/{user}"
            };

            meta.AddPathParameter("user", "bart simpson");

            WebRequest request = CreateRequest(meta);

            Assert.Equals("http://example.com/users/bart%20simpson", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Tests that query parameters can be added to a request Uri
        /// </summary>
        [Test]
        public void Can_add_query_parameters_to_request()
        {
            MethodMetadata meta = new MethodMetadata();
            meta.AddQueryParameter("q", "test");
            meta.AddQueryParameter("q2", "test2");

            WebRequest request = CreateRequest(meta);

            Assert.Equals("http://example.com/?q=test&q2=test2", request.RequestUri.AbsoluteUri);
        }

        private WebRequest CreateRequest(MethodMetadata metadata)
        {
            IMediaTypeHandlerRegistry registry = new DefaultMediaTypeRegistry();
            return metadata.CreateRequest(TestUri, Credentials, registry);
        }
    }
}
