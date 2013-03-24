// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Ensure.cs">
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
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// A set of constraints to act as preconditions for method invocations.
    /// </summary>
    internal static class Ensure
    {
        /// <summary>
        /// Raises an <see cref="ArgumentNullException"/> when <paramref name="instance"/> is null.
        /// </summary>
        /// <param name="instance"> The instance to inspect </param>
        /// <param name="parameterName"> The parameter name. </param>
        /// <typeparam name="T"> The type of instance </typeparam>
        /// <exception cref="ArgumentNullException">Raised when <paramref name="instance"/> is null </exception>
        public static void IsNotNull<T>(T instance, string parameterName) where T : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Raises an <see cref="ArgumentNullException"/> when <paramref name="value"/> is null, 
        /// and an <see cref="ArgumentException"/> when <paramref name="value"/>is an empty string.
        /// </summary>
        /// <param name="value"> The value to test. </param>
        /// <param name="paramName"> The parameter name. </param>
        /// <exception cref="ArgumentNullException">Raised when <paramref name="value"/> is null </exception>
        /// <exception cref="ArgumentException"> Raised when value is an empty string </exception>
        public static void IsNotNullOrEmpty(string value, string paramName)
        {
            IsNotNull(value, paramName);
            if (value.Length == 0)
            {
                throw new ArgumentException(string.Format("{0} cannot be an empty string", paramName), paramName);
            }
        }

        /// <summary>
        /// Verifies the precondition and throws an <see cref="ArgumentException"/> with the given
        /// message when the precondition returns false.
        /// </summary>
        /// <param name="predicate"> The predicate to test. </param>
        /// <param name="message"> The exception message to raise when the predicate returns false. </param>
        public static void That(Func<bool> predicate, string message)
        {
            if (!predicate())
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Verifies the precondition and throws an <see cref="ArgumentException"/> with the given
        /// message when the precondition returns false.
        /// </summary>
        /// <param name="predicate"> The predicate to test. </param>
        /// <param name="message"> The exception message to raise when the predicate returns false. </param>
        public static void That(bool predicate, string message)
        {
            if (!predicate)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
