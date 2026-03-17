// -----------------------------------------------------------------------
// <copyright file="MethodMetadataTests.cs">
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
// ------------------------------------------------------------------------------------

using EasyPeasy.Http;
using EasyPeasy.Implementation;

using Xunit;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace EasyPeasy.Tests.Implementation
{
    /// <summary>
    /// A set of tests for the <see cref="MethodMetadata"/> class.
    /// </summary>
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
        [Fact]
        public void Accept_header_defaults_to_consumes_property()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.Consumes = "application/json";

            var (request, wrapper) = CreateRequest(metadata);

            Assert.Equal("application/json", request.Headers.Accept.FirstOrDefault()?.MediaType);
        }

        /// <summary>
        /// The [Verb] attribute sets the HTTP method to use
        /// </summary>
        [Fact]
        public void Request_method_is_set_to_verb_property()
        {
            MethodMetadata metadata = new MethodMetadata { Verb = HttpVerb.DELETE };

            var (request, wrapper) = CreateRequest(metadata);

            Assert.Equal("DELETE", request.Method.Method);
        }

        /// <summary>
        /// The [Verb] attribute sets the HTTP method to use
        /// </summary>
        [Fact]
        public void Uri_is_set_on_request()
        {
            MethodMetadata metadata = new MethodMetadata();

            var (request, wrapper) = CreateRequest(metadata);

            Assert.Equal("http://example.com/", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Verifies the wrapper has the correct properties set
        /// </summary>
        [Fact]
        public void Wrapper_has_correct_properties()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.Consumes = "application/json";

            var (request, wrapper) = CreateRequest(metadata);

            Assert.NotNull(wrapper.RequestUri);
            Assert.Equal("GET", wrapper.Method);
            Assert.Equal("application/json", wrapper.Accept);
        }

        [Fact]
        public void Can_add_header_parameter()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.AddHeaderParameter("Header-Name", "Header-Value");

            var (request, wrapper) = CreateRequest(metadata);

            Assert.Equal("Header-Value", request.Headers.GetValues("Header-Name").FirstOrDefault());
        }

        /// <summary>
        /// Accept header needs to be special cased (not added to the headers
        /// collection directly)
        /// </summary>
        [Fact]
        public void Can_add_accept_header()
        {
            MethodMetadata metadata = new MethodMetadata();
            metadata.AddHeaderParameter("Accept", "application/json");

            var (request, wrapper) = CreateRequest(metadata);

            Assert.Equal("application/json", request.Headers.Accept.FirstOrDefault()?.MediaType);
        }

        /// <summary>
        /// Tests that the request URI is comprised of both the base Uri and the service path
        /// </summary>
        [Fact]
        public void Full_uri_uses_service_path()
        {
            MethodMetadata meta = new MethodMetadata { ServicePath = "/service/1.0" };

            var (request, wrapper) = CreateRequest(meta);

            Assert.Equal("http://example.com/service/1.0/", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Tests that the Path attribute added to the method is included in the URI
        /// </summary>
        [Fact]
        public void Method_path_is_appended_to_uri()
        {
            MethodMetadata meta = new MethodMetadata
            {
                ServicePath = "api/v1",
                MethodPath = "/action"
            };

            var (request, wrapper) = CreateRequest(meta);

            Assert.Equal("http://example.com/api/v1/action", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Verifes that templated parts of a URI can be substituted with path parameters
        /// </summary>
        [Fact]
        public void Uri_can_be_substituted_with_path_params()
        {
            MethodMetadata meta = new MethodMetadata
            {
                ServicePath = "services/{apiVersion}",
                MethodPath = "/users/{user}"
            };

            meta.AddPathParameter("apiVersion", "1.0");
            meta.AddPathParameter("user", "matt");

            var (request, wrapper) = CreateRequest(meta);

            Assert.Equal("http://example.com/services/1.0/users/matt", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Verifies that values provided in path parameters are escaped correctly
        /// </summary>
        [Fact]
        public void Uri_is_escaped()
        {
            MethodMetadata meta = new MethodMetadata
            {
                MethodPath = "/users/{user}"
            };

            meta.AddPathParameter("user", "bart simpson");

            var (request, wrapper) = CreateRequest(meta);

            Assert.Equal("http://example.com/users/bart%20simpson", request.RequestUri.AbsoluteUri);
        }

        /// <summary>
        /// Tests that query parameters can be added to a request Uri
        /// </summary>
        [Fact]
        public void Can_add_query_parameters_to_request()
        {
            MethodMetadata meta = new MethodMetadata();
            meta.AddQueryParameter("q", "test");
            meta.AddQueryParameter("q2", "test2");

            var (request, wrapper) = CreateRequest(meta);

            Assert.Equal("http://example.com/?q=test&q2=test2", request.RequestUri.AbsoluteUri);
        }

        private (HttpRequestMessage, HttpRequestWrapper) CreateRequest(MethodMetadata metadata)
        {
            IMediaTypeHandlerRegistry registry = new DefaultMediaTypeRegistry();
            return metadata.CreateRequest(TestUri, Credentials, registry);
        }
    }
}
