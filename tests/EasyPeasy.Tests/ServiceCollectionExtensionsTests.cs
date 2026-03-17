// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensionsTests.cs">
//   The MIT License (MIT)
//     Copyright © 2013 Matt Channer (mchanner at gmail dot com)
//
//     Permission is hereby granted, free of charge, to any person obtaining a
//     copy of this software and associated documentation files (the "Software"),
//     to deal in the Software without restriction, including without limitation
//     the rights to use, copy, modify, merge, publish, distribute, sublicense,
//     and/or sell copies of the Software, and to permit persons to whom the
//     Software is furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included
//     in all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//     OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
//     THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace EasyPeasy.Tests
{
    /// <summary>
    /// Tests for the <see cref="ServiceCollectionExtensions"/> class.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Verifies that AddEasyPeasy registers the factory and registry.
        /// </summary>
        [Fact]
        public void AddEasyPeasy_Registers_Factory_And_Registry()
        {
            var services = new ServiceCollection();

            services.AddEasyPeasy();

            var provider = services.BuildServiceProvider();

            var factory = provider.GetService<IEasyPeasyFactory>();
            var registry = provider.GetService<IMediaTypeHandlerRegistry>();

            Assert.NotNull(factory);
            Assert.NotNull(registry);
            Assert.IsType<EasyPeasyFactory>(factory);
            Assert.IsType<DefaultMediaTypeRegistry>(registry);
        }

        /// <summary>
        /// Verifies that AddEasyPeasy with custom registry uses that registry.
        /// </summary>
        [Fact]
        public void AddEasyPeasy_With_Custom_Registry_Uses_Custom_Registry()
        {
            var services = new ServiceCollection();
            var customRegistry = new DefaultMediaTypeRegistry();

            services.AddEasyPeasy(customRegistry);

            var provider = services.BuildServiceProvider();

            var registry = provider.GetService<IMediaTypeHandlerRegistry>();

            Assert.Same(customRegistry, registry);
        }

        /// <summary>
        /// Verifies that factory is registered as singleton.
        /// </summary>
        [Fact]
        public void AddEasyPeasy_Registers_Factory_As_Singleton()
        {
            var services = new ServiceCollection();

            services.AddEasyPeasy();

            var provider = services.BuildServiceProvider();

            var factory1 = provider.GetService<IEasyPeasyFactory>();
            var factory2 = provider.GetService<IEasyPeasyFactory>();

            Assert.Same(factory1, factory2);
        }

        /// <summary>
        /// Verifies that registry is registered as singleton.
        /// </summary>
        [Fact]
        public void AddEasyPeasy_Registers_Registry_As_Singleton()
        {
            var services = new ServiceCollection();

            services.AddEasyPeasy();

            var provider = services.BuildServiceProvider();

            var registry1 = provider.GetService<IMediaTypeHandlerRegistry>();
            var registry2 = provider.GetService<IMediaTypeHandlerRegistry>();

            Assert.Same(registry1, registry2);
        }

        /// <summary>
        /// Verifies that the method returns the service collection for chaining.
        /// </summary>
        [Fact]
        public void AddEasyPeasy_Returns_ServiceCollection_For_Chaining()
        {
            var services = new ServiceCollection();

            var result = services.AddEasyPeasy();

            Assert.Same(services, result);
        }
    }
}
