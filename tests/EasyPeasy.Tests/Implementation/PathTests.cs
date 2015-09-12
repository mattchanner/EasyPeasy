// -----------------------------------------------------------------------
// <copyright file="PathTests.cs">
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
// ------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

using EasyPeasy.Implementation;

using NUnit.Framework;

namespace EasyPeasy.Tests.Implementation
{
    /// <summary>
    /// A set of tests for the <see cref="Path"/> class.
    /// </summary>
    [TestFixture]
    public class PathTests
    {
        /// <summary>
        /// The default path constructor should set the path as '/'
        /// </summary>
        [Test]
        public void Default_constructor_sets_path_as_root()
        {
            Assert.That(new Path().FullPath, Is.EqualTo("/"));
        }

        /// <summary>
        /// Ensures that passing in a path value to the constructor applies this as the full path value
        /// </summary>
        [Test]
        public void Path_constructor_stores_constructor_argument_as_path_value()
        {
            Assert.That(new Path("/path/to/resources").FullPath, Is.EqualTo("/path/to/resources"));
        }

        /// <summary>
        /// Where the path contains parameterized values, these values are extracted and stored in the group collection
        /// </summary>
        [Test]
        public void Can_extract_variable_names_from_supplied_path()
        {
            Path p = new Path("/{name}/{name2}");
            List<string> pathNames = p.VariableNames.ToList();
            Assert.That(pathNames.Count, Is.EqualTo(2));
            Assert.That(pathNames.Contains("name"), Is.True);
            Assert.That(pathNames.Contains("name2"), Is.True);
        }

        /// <summary>
        /// Tests that non balanced braces are ignored
        /// </summary>
        [Test]
        public void Variable_names_do_not_contain_duplicates()
        {
            Path p = new Path("/{name}/{name}");
            List<string> pathNames = p.VariableNames.ToList();
            Assert.That(pathNames.Count, Is.EqualTo(1));
            Assert.That(pathNames[0], Is.EqualTo("name"));
        }

        /// <summary>
        /// Tests that appending 2 paths, where p1 is not terminator with a separator and p2 does not start
        /// with a separator produces a valid path string
        /// </summary>
        [Test]
        public void Can_append_paths_with_no_leading_or_trailing_separator()
        {
            Path p1 = new Path("/first/path");
            Path p2 = new Path("second/path");

            Path combined = p1.Append(p2);
            Assert.That(combined.FullPath, Is.EqualTo("/first/path/second/path"));
        }

        /// <summary>
        /// Appending 2 paths where the first contains a trailing separator
        /// </summary>
        [Test]
        public void Can_append_paths_with_trailing_separator_on_first_arg()
        {
            Path p1 = new Path("/first/path/");
            Path p2 = new Path("second/path");

            Path combined = p1.Append(p2);
            Assert.That(combined.FullPath, Is.EqualTo("/first/path/second/path"));
        }

        /// <summary>
        /// Appending 2 paths where the second contains a leading separator
        /// </summary>
        [Test]
        public void Can_append_paths_with_leading_separator_on_second_path()
        {
            Path p1 = new Path("/first/path");
            Path p2 = new Path("/second/path");

            Path combined = p1.Append(p2);
            Assert.That(combined.FullPath, Is.EqualTo("/first/path/second/path"));
        }

        /// <summary>
        /// Appending 2 paths where the first contains a trailing separator
        /// </summary>
        [Test]
        public void Can_append_paths_with_trailing_and_leading_separators()
        {
            Path p1 = new Path("/first/path/");
            Path p2 = new Path("/second/path");

            Path combined = p1.Append(p2);
            Assert.That(combined.FullPath, Is.EqualTo("/first/path/second/path"));
        }

        /// <summary>
        /// Ensures the ToString() method can be used interchangeably with the FullPath property
        /// </summary>
        [Test]
        public void Path_to_string_returns_path()
        {
            Path p = new Path("/path/to/resource");
            Assert.AreEqual(p.FullPath, p.ToString());
        }

        /// <summary>
        /// Path variables can differ by case
        /// </summary>
        [Test]
        public void Replace_path_variables_is_case_sensitive()
        {
            Path p = new Path("/{name}/{NAME}");
            IDictionary<string, object> mapping = new Dictionary<string, object>
                {
                    { "name", "lowercase" },
                    { "NAME", "UPPERCASE" },
                };
            Path specializedPath = p.ReplacePathVariables(mapping);
            Assert.That(specializedPath.FullPath, Is.EqualTo("/lowercase/UPPERCASE"));
        }

        /// <summary>
        /// If the same variable appears multiple times in a path, all instances are replaced with the same value
        /// </summary>
        [Test]
        public void Repeating_names_are_replaced_with_same_value()
        {
            Path p = new Path("/{id}/sub-resource/{id}");
            IDictionary<string, object> mapping = new Dictionary<string, object> { { "id", "replaced" } };
            Path specializedPath = p.ReplacePathVariables(mapping);
            Assert.That(specializedPath.FullPath, Is.EqualTo("/replaced/sub-resource/replaced"));
        }

        /// <summary>
        /// An exception is raised when the path contains a variable that does not have a suitable mapping
        /// </summary>
        [Test, ExpectedException(typeof(EasyPeasyException))]
        public void Unknown_path_params_throw_EasyPeasyException()
        {
            Path p = new Path("/{name}/{unknown}");
            IDictionary<string, object> mapping = new Dictionary<string, object>
                {
                    { "name", "replaced" }
                };

            p.ReplacePathVariables(mapping);
        }
    }
}
