﻿// -----------------------------------------------------------------------
// <copyright file="PathAttribute.cs">
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
    /// The path attribute is used at the interface level to define the root path
    /// of the service
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface)]
    public class PathAttribute : Attribute
    {
        /// <summary> The path value </summary>
        private readonly string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathAttribute"/> class.
        /// </summary>
        /// <param name="relativePath"> The relative path. </param>
        public PathAttribute(string relativePath)
        {
            this.path = relativePath;
        }

        /// <summary>
        /// Gets the relative path to the service.
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
        }
    }
}
