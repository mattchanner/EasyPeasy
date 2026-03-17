// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs">
//
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

namespace EasyPeasy
{
    /// <summary>
    /// Extension methods for registering EasyPeasy services with Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds EasyPeasy services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddEasyPeasy(this IServiceCollection services)
        {
            services.AddSingleton<IMediaTypeHandlerRegistry, DefaultMediaTypeRegistry>();
            services.AddSingleton<IEasyPeasyFactory, EasyPeasyFactory>();

            return services;
        }

        /// <summary>
        /// Adds EasyPeasy services to the specified <see cref="IServiceCollection"/> with a custom registry.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="registry">The custom media type handler registry to use.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        public static IServiceCollection AddEasyPeasy(this IServiceCollection services, IMediaTypeHandlerRegistry registry)
        {
            services.AddSingleton(registry);
            services.AddSingleton<IEasyPeasyFactory, EasyPeasyFactory>();

            return services;
        }
    }
}
