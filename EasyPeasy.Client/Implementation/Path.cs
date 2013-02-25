// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Path.cs">
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyPeasy.Client.Implementation
{
    /// <summary>
    /// Represents a path to a server resources
    /// </summary>
    public class Path
    {
        /// <summary> The path separator </summary>
        private const string PathSeperator = "/";

        /// <summary> The default path for the root when no path is provided </summary>
        private const string DefaultRoot = PathSeperator;

        /// <summary> The regex to use for matching elements in a path </summary>
        private static readonly Regex NameMatcher = new Regex("{(?<name>.*?)}", RegexOptions.Compiled);

        /// <summary> The list of variable names extracted from the path </summary>
        private readonly List<Group> variableNames;

        /// <summary> The root path </summary>
        private string rootPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        public Path() : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Path"/> class.
        /// </summary>
        /// <param name="rootPath">The root path</param>
        public Path(string rootPath)
        {
            this.rootPath = string.IsNullOrEmpty(rootPath) ? DefaultRoot : rootPath;
            this.variableNames = ExtractVariableNames(this.rootPath);
        }

        /// <summary>
        /// Gets the root path
        /// </summary>
        public string FullPath
        {
            get { return rootPath; }
        }

        /// <summary>
        /// Gets the read only list of extracted variable names
        /// </summary>
        public IEnumerable<string> VariableNames
        {
            get { return new HashSet<string>(this.variableNames.Select(g => g.Value)); }
        }

        /// <summary>
        /// Joins this path with another, returning the result as a new path instance.
        /// </summary>
        /// <param name="other">The other path to join to</param>
        /// <returns>The new <see cref="Path"/> instance</returns>
        public Path Append(Path other)
        {
            if (this.rootPath.EndsWith(PathSeperator))
            {
                if (other.FullPath.StartsWith(PathSeperator))
                    return new Path(this.rootPath + other.rootPath.Substring(1));
                else
                    return new Path(this.rootPath + other.rootPath);
            }
            else
            {
                if (other.FullPath.StartsWith(PathSeperator))
                    return new Path(this.rootPath + other.rootPath);
                else
                    return new Path(this.rootPath + PathSeperator + other.rootPath);
            }
        }

        /// <summary>
        /// Generates a new <see cref="Path"/> with any placeholder values replaced with values derived from
        /// the source mappings
        /// </summary>
        /// <param name="mapping">The dictionary containing entries where each key represents a variable
        /// in the path.</param>
        /// <returns>The new path instance with the placeholder variables replaced with their mapped values</returns>
        public Path ReplacePathVariables(IDictionary<string, object> mapping)
        {
            if (this.variableNames.Count == 0)
                return this;

            StringBuilder pathBuilder = new StringBuilder();

            int previousIndex = 0;

            foreach (Group group in this.variableNames)
            {
                // Append previous section to the builder
                if (group.Index > 0)
                {
                    pathBuilder.Append(this.rootPath.Substring(previousIndex, group.Index - 1 - previousIndex));
                }

                object mappedValue;
                if (mapping.TryGetValue(group.Value, out mappedValue))
                {
                    string mappedString = mappedValue == null ? string.Empty : mappedValue.ToString();
                    pathBuilder.Append(mappedString);
                }
                else
                {
                    throw new EasyPeasyException(string.Format("Path contains an unknown parameter of '{0}'", group.Value));
                }

                previousIndex = group.Index + group.Length + 1;
            }

            Group lastGroup = variableNames[variableNames.Count - 1];
            int endPosition = lastGroup.Index + lastGroup.Length;
            if (this.rootPath.Length > endPosition)
            {
                pathBuilder.Append(this.rootPath.Substring(endPosition + 1));
            }

            return new Path(pathBuilder.ToString());
        }

        /// <summary>
        /// Returns a string representation of this path instance.
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            return this.FullPath;
        }

        /// <summary>
        /// Extracts any variable names from the given path and returns them as a list
        /// </summary>
        /// <param name="path">The path to extract</param>
        /// <returns>The list of variables names</returns>
        private List<Group> ExtractVariableNames(string path)
        {
            var names = new List<Group>();

            var matches = NameMatcher.Matches(path);

            foreach (Match match in matches)
            {
                Group group = match.Groups["name"];
                names.Add(group);
            }

            return names;
        }
    }
}
