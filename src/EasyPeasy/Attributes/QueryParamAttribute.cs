// -----------------------------------------------------------------------
// <copyright file="QueryParamAttribute.cs">
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
// ----------------------------------------------------------------------------

using System;

namespace EasyPeasy.Attributes
{
    /// <summary>
    /// Represents a named query string parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class QueryParamAttribute : Attribute
    {
        /// <summary> The parameter name. </summary>
        private readonly string parameterName;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParamAttribute"/> class.
        /// </summary>
        /// <param name="parameterName">
        /// The parameter name.
        /// </param>
        public QueryParamAttribute(string parameterName)
        {
            this.parameterName = parameterName;
        }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName
        {
            get { return this.parameterName; }
        }
    }
}
