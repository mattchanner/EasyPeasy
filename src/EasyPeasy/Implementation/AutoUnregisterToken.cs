// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoUnregisterToken.cs">
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

using System;

namespace EasyPeasy.Implementation
{
    /// <summary>
    /// Instances of this class will unregister an interceptor from the supplied factory when the Dispose method is called.
    /// </summary>
    internal class AutoUnregisterToken : IDisposable
    {
        /// <summary> The factory to remove from. </summary>
        private readonly IEasyPeasyFactory factory;

        /// <summary> The interceptor to remove. </summary>
        private readonly IRequestInterceptor interceptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoUnregisterToken"/> class.
        /// </summary>
        /// <param name="factory"> The factory to unregister from. </param>
        /// <param name="interceptor"> The interceptor to remove on dispose. </param>
        public AutoUnregisterToken(IEasyPeasyFactory factory, IRequestInterceptor interceptor)
        {
            Ensure.IsNotNull(factory, "factory");
            Ensure.IsNotNull(interceptor, "interceptor");

            this.factory = factory;
            this.interceptor = interceptor;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            factory.RemoveInterceptor(interceptor);
        }
    }
}
