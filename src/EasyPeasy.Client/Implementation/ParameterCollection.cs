// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterCollection.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// Represents a collection of parameters, and their associated values
    /// </summary>
    public class ParameterCollection : IEnumerable<KeyValuePair<string, IList<object>>>
    {
        /// <summary> The parameters. </summary>
        private readonly IDictionary<string, IList<object>> parameters = new Dictionary<string, IList<object>>();

        /// <summary>
        /// Creates a new instance of the <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection()
        {
        }


        /// <summary>
        /// Gets the number of parameters in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return parameters.Count;
            }
        }
        
        /// <summary>
        /// Adds a new query string parameter
        /// </summary>
        /// <param name="paramName">The parameter name</param>
        /// <param name="paramValue">The parameter value</param>
        /// <returns>A reference to this instance</returns>
        /// <exception cref="ArgumentNullException">Raised if <paramref name="paramName" /> is null</exception>
        /// <exception cref="ArgumentException">Raised if <paramref name="paramName" /> is an empty string</exception>
        /// <exception cref="DuplicateKeyException">Raised if <paramref name="paramName" /> already exists</exception>
        public ParameterCollection Add(string paramName, object paramValue)
        {
            Ensure.IsNotNullOrEmpty(paramName, "paramName");
            Ensure.IsNotNull(paramValue, "paramValue");

            IList<object> valueList;

            if (!this.parameters.TryGetValue(paramName, out valueList))
            {
                valueList = new List<object>();
                this.parameters[paramName] = valueList;
            }

            // Unpack sequences and add items individually
            IEnumerable valueSequence = paramValue as IEnumerable;
            if (valueSequence != null && !(paramValue is string))
            {
                foreach (object value in valueSequence) valueList.Add(value);
            }
            else
            {
                valueList.Add(paramValue);
            }

            return this;
        }

        /// <summary>
        /// Adds each item in the sequence to the query string.
        /// </summary>
        /// <param name="keysAndValues">The sequence of keys and values to add</param>
        /// <returns>The current <see cref="ParameterCollection"/> instance</returns>
        public ParameterCollection AddAll(IEnumerable<Tuple<string, object>> keysAndValues)
        {
            foreach (Tuple<string, object> keyAndValue in keysAndValues)
            {
                Add(keyAndValue.Item1, keyAndValue.Item2);
            }

            return this;
        }

        /// <summary>
        /// Conditionally adds the parameter value if it is set to a non empty string.
        /// </summary>
        /// <param name="paramName"> The parameter name. </param>
        /// <param name="paramValue"> The parameter value. </param>
        /// <returns> The <see cref="ParameterCollection"/>. </returns>
        /// <exception cref="ArgumentNullException">Raised if <paramref name="paramName" /> is null</exception>
        /// <exception cref="ArgumentException">Raised if <paramref name="paramName" /> is an empty string</exception>
        /// <exception cref="DuplicateKeyException">Raised if <paramref name="paramName" /> already exists</exception>
        public ParameterCollection MaybeAdd(string paramName, object paramValue)
        {
            Ensure.IsNotNullOrEmpty(paramName, "paramName");

            if (paramValue != null)
            {
                this.Add(paramName, paramValue);
            }

            return this;
        }

        /// <summary>
        /// Returns a new path containing the provided path, and the query string parameters
        /// held within the instance
        /// </summary>
        /// <param name="path"> The path to use as a the basis for the new path </param>
        /// <returns>
        /// The <see cref="string"/> containing the path, and the query string parameters
        /// </returns>
        public string AppendToPath(string path)
        {
            Ensure.IsNotNullOrEmpty(path, "path");

            if (!path.EndsWith("?"))
            {
                path += "?";
            }

            return path + this;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<KeyValuePair<string, IList<object>>> GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        /// <summary>
        /// Returns a string representation of the query string instance
        /// </summary>
        /// <returns>The query string parameters as a name value pair list</returns>
        public override string ToString()
        {
            if (this.parameters.Count == 0) return string.Empty;

            return (from kv in this.parameters
                    from value in kv.Value
                    where value != null
                    select string.Format("{0}={1}", kv.Key, this.ParameterToString(value))).Aggregate(
                        (s, s1) => string.Format("{0}&{1}", s, s1));
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns a string representation of the parameter object
        /// </summary>
        /// <param name="value">The value to serialize</param>
        /// <returns>The string representation</returns>
        private object ParameterToString(object value)
        {
            // TODO: Need to make culture configurable
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
    }
}