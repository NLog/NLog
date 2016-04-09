// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

namespace SyncProjectItems
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Read ProjectFileInfo.xml and sync .csproj files (defined in ProjectFileInfo.xml)
    /// </summary>
    class Program
    {
        private static XNamespace MSBuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                string projectFileName = Path.GetFullPath(arg);
                string baseDirectory = Path.GetDirectoryName(projectFileName);
                XElement projectDescriptor = XElement.Load(projectFileName);

                ProcessProjectDescriptor(projectDescriptor, baseDirectory);
            }
        }

        private static void ProcessProjectDescriptor(XElement projectDescriptor, string baseDirectory)
        {
            Console.WriteLine("Processing project files in '{0}':", baseDirectory);
            var filesets = LoadFileSets(projectDescriptor, baseDirectory);
            Console.WriteLine("File sets:");
            foreach (var fs in filesets)
            {
                Console.WriteLine("  {0}: {1} files", fs.Key, fs.Value.Count);
            }

            foreach (var project in projectDescriptor.Elements("Project"))
            {
                string file = (string)project.Attribute("File");
                string projectFileName = Path.Combine(baseDirectory, file);
                var projectContents = XElement.Load(projectFileName);

                Console.WriteLine("Updating file: {0}", file);
                string contentBefore = projectContents.ToString();

                foreach (var itemGroup in project.Elements("ItemGroup"))
                {
                    string itemGroupName = (string)itemGroup.Attribute("Name");
                    var keepOldAttr = itemGroup.Attribute("KeepOldFiles");
                    bool keepOldFiles = keepOldAttr != null && bool.Parse(keepOldAttr.Value);
                    var contents = new HashSet<string>();
                    foreach (var fileSetElement in itemGroup.Elements("FileSet"))
                    {
                        string include = (string)fileSetElement.Attribute("Include");
                        string exclude = (string)fileSetElement.Attribute("Exclude");

                        if (include != null)
                        {
                            var fileset = GetFileset(filesets, include);
                            contents.UnionWith(fileset);
                        }

                        if (exclude != null)
                        {
                            var fileset = GetFileset(filesets, exclude);
                            contents.ExceptWith(fileset);
                        }
                    }

                    Console.WriteLine("  <{0}/>: {1} items", itemGroupName, contents.Count);

                    var fullItemGroup = MSBuildNamespace + "ItemGroup";
                    var fullItemGroupName = MSBuildNamespace + itemGroupName;
                    var projectItemGroup = projectContents.Elements(fullItemGroup).FirstOrDefault(c => c.Elements(fullItemGroupName).Any());
                    var groupIsEmpty = false;

                    if (projectItemGroup != null)
                    {
                        //remove old elemens
                        if (!keepOldFiles)
                        {
                            projectItemGroup.Elements().Remove();
                            groupIsEmpty = true;
                        }
                    }
                    else
                    {
                        //create new group
                        projectItemGroup = new XElement(fullItemGroup);
                        projectContents.Add(projectItemGroup);
                        groupIsEmpty = true;
                    }
                    var xElementComparer = new XElementIncludeAttrComparer();
                    //add files
                    foreach (var filename in contents.OrderBy(c => c))
                    {
                        var item = new XElement(fullItemGroupName, new XAttribute("Include", filename));

                        if (!groupIsEmpty)
                        {
                            //ignore if already there
                           
                            if (projectItemGroup.Elements().Contains(item, xElementComparer))
                                break;
                        }

                        foreach (var customize in projectDescriptor.Elements(MSBuildNamespace + "Customize"))
                        {
                            string fileSet = (string)customize.Attribute("FileSet");
                            var fsContent = filesets[fileSet];
                            if (fsContent.Contains(filename))
                            {
                                foreach (var e in customize.Elements())
                                {
                                    var cust = XElement.Parse(e.ToString());
                                    cust.Attribute(XNamespace.None + "xmlns").Remove();
                                    item.Add(cust);
                                }
                            }
                        }

                        projectItemGroup.Add(item);
                    }

                }

                string contentAfter = projectContents.ToString();
                if (contentBefore != contentAfter)
                {
                    Console.WriteLine("  Project updated. Saving.");
                    projectContents.Save(projectFileName);
                }
                else
                {
                    Console.WriteLine("  Project file is up-to-date.");
                }
            }
        }

        private static IEnumerable<string> GetFileset(Dictionary<string, HashSet<string>> filesets, string name)
        {
            if (!filesets.ContainsKey(name))
            {
                throw new Exception(string.Format("Fileset with key {0} is not found (case sensitive)", name));
            }

            return filesets[name];
        }

        private static Dictionary<string, HashSet<string>> LoadFileSets(XElement projectDescriptor, string baseDirectory)
        {
            var filesets = new Dictionary<string, HashSet<string>>();
            foreach (var fileSetElement in projectDescriptor.Elements("FileSet"))
            {
                string name = (string)fileSetElement.Attribute("Name");
                var fileList = new HashSet<string>();

                foreach (var includeElement in fileSetElement.Elements("Include"))
                {
                    string fileName = (string)includeElement.Attribute("File");
                    if (fileName.Contains("*"))
                    {
                        FindMatchingFiles(fileList, fileName, baseDirectory, string.Empty, true);
                    }
                    else
                    {
                        fileList.Add(fileName);
                    }
                }

                foreach (var includeElement in fileSetElement.Elements("Exclude"))
                {
                    string fileName = (string)includeElement.Attribute("File");
                    if (fileName.Contains("*"))
                    {
                        FindMatchingFiles(fileList, fileName, baseDirectory, string.Empty, false);
                    }
                    else
                    {
                        fileList.Remove(fileName);
                    }
                }

                filesets[name] = fileList;
            }
            return filesets;
        }

        private static void FindMatchingFiles(HashSet<string> fileList, string fileName, string directory, string prefix, bool add)
        {
            foreach (var file in Directory.GetFiles(directory, fileName))
            {
                string baseName = Path.GetFileName(file);
                if (add)
                {
                    fileList.Add(prefix + baseName);
                }
                else
                {
                    fileList.Remove(prefix + baseName);
                }

            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                string baseName = Path.GetFileName(dir);
                if (baseName == "bin" || baseName == "obj")
                {
                    continue;
                }

                FindMatchingFiles(fileList, fileName, dir, prefix + baseName + "\\", add);
            }
        }

        /// <summary>
        /// Compare xelemens by inlude attr
        /// </summary>
        private class XElementIncludeAttrComparer : IEqualityComparer<XElement>
        {
           
            #region Implementation of IEqualityComparer<in XElement>

            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            /// <param name="x">The first object of type <paramref name="T"/> to compare.</param><param name="y">The second object of type <paramref name="T"/> to compare.</param>
            public bool Equals(XElement x, XElement y)
            {
                if (x == null) return y == null;

                var includeXAttr = x.Attribute("Include");
                var includeYAttr = y.Attribute("Include");

                if (includeXAttr == null) return includeYAttr == null;
                return  x.Name == y.Name && includeXAttr.Value == includeYAttr.Value;
            }

            /// <summary>
            /// Returns a hash code for the specified object.
            /// </summary>
            /// <returns>
            /// A hash code for the specified object.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param><exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
            public int GetHashCode(XElement obj)
            {
                throw new NotSupportedException();
            }

            #endregion



        }
    }

    
}
