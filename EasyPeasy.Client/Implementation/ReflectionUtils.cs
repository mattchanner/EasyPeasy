// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionUtils.cs">
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
using System.Linq;
using System.Reflection;

using EasyPeasy.Attributes;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// A set of reflection based static methods
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Returns the HttpVerb based on method attributes.
        /// </summary>
        /// <param name="method">The method to query</param>
        /// <returns>The inferred verb</returns>
        public static HttpVerb DetermineHttpVerb(MethodInfo method)
        {
            object[] attributes = method.GetCustomAttributes(false);

            foreach (object attribute in attributes)
            {
                if (attribute is GETAttribute) return HttpVerb.GET;
                if (attribute is PUTAttribute) return HttpVerb.PUT;
                if (attribute is POSTAttribute) return HttpVerb.POST;
                if (attribute is DELETEAttribute) return HttpVerb.DELETE;
            }

            return HttpVerb.GET;
        }

        /// <summary>
        /// Queries the method for the Consumes attribute, returning the found value.  If the
        /// method does not contain such an attribute, the containing type is queried.
        /// </summary>
        /// <param name="method"> The method to inspect. </param>
        /// <param name="defaultWhenNotPresent"> The default to return when not present. </param>
        /// <returns> The media type </returns>
        public static string DetermineConsumesMediaType(MethodInfo method, string defaultWhenNotPresent)
        {
            ConsumesAttribute methodAttribute = GetAttribute<ConsumesAttribute>(method);
            if (methodAttribute != null) return methodAttribute.MediaType;

            ConsumesAttribute parentAttribute = GetAttribute<ConsumesAttribute>(method.DeclaringType);
            if (parentAttribute != null) return parentAttribute.MediaType;

            return defaultWhenNotPresent;
        }

        /// <summary>
        /// Queries the method for the Produces attribute, returning the found value.  If the
        /// method does not contain such an attribute, the containing type is queried.
        /// </summary>
        /// <param name="method"> The method to inspect. </param>
        /// <param name="defaultWhenNotPresent"> The default to return when not present. </param>
        /// <returns> The media type </returns>
        public static string DetermineProducesMediaType(MethodInfo method, string defaultWhenNotPresent)
        {
            ProducesAttribute methodAttribute = GetAttribute<ProducesAttribute>(method);
            if (methodAttribute != null) return methodAttribute.MediaType;

            ProducesAttribute parentAttribute = GetAttribute<ProducesAttribute>(method.DeclaringType);
            if (parentAttribute != null) return parentAttribute.MediaType;

            return defaultWhenNotPresent;
        }

        /// <summary>
        /// Returns a value indicating whether the extended type represents a void type.
        /// </summary>
        /// <param name="type"> The type. </param>
        /// <returns>True if the type is a void type </returns>
        public static bool IsVoid(this Type type)
        {
            return type == typeof(void);
        }

        /// <summary>
        /// Gets an attribute associated to the method, or returns null if the attribute does not exist.
        /// </summary>
        /// <param name="method"> The method to query. </param>
        /// <typeparam name="TAttr"> The attribute type to get </typeparam>
        /// <returns> The <see cref="TAttr">attribute</see>, or null if not found. </returns>
        public static TAttr GetAttribute<TAttr>(MethodInfo method)
        {
            return method.GetCustomAttributes(false).OfType<TAttr>().FirstOrDefault();
        }

        /// <summary>
        /// Gets an attribute associated to the parameter   , or returns null if the attribute does not exist.
        /// </summary>
        /// <param name="parameter"> The parameter to query. </param>
        /// <typeparam name="TAttr"> The attribute type to get </typeparam>
        /// <returns> The <see cref="TAttr">attribute</see>, or null if not found. </returns>
        public static TAttr GetAttribute<TAttr>(ParameterInfo parameter)
        {
            return parameter.GetCustomAttributes(false).OfType<TAttr>().FirstOrDefault();
        }

        /// <summary>
        /// Gets an attribute associated to the method, or returns null if the attribute does not exist.
        /// </summary>
        /// <param name="type"> The method to query. </param>
        /// <typeparam name="TAttr"> The attribute type to get </typeparam>
        /// <returns> The <see cref="TAttr">attribute</see>, or null if not found. </returns>
        public static TAttr GetAttribute<TAttr>(Type type)
        {
            return type.GetCustomAttributes(false).OfType<TAttr>().FirstOrDefault();
        }
    }
}
