// 
// Copyright (c) 2004-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
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

using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;
using NAnt.Core.Util;

namespace Tools
{
    [TaskName("sync-vs-project-items")]
    public class SyncVSProjectItems : Task
    {
        private FileSet _sourceFiles;
        private FileSet _projectFiles;
        private FileSet _resourceFiles;

        [BuildElement("source-files")]
        public FileSet SourceFiles {
            get { return _sourceFiles; }
            set { _sourceFiles = value; }
        }

        [BuildElement("project-files")]
        public FileSet ProjectFiles {
            get { return _projectFiles; }
            set { _projectFiles = value; }
        }

        [BuildElement("resource-files")]
        public FileSet ResourceFiles {
            get { return _resourceFiles; }
            set { _resourceFiles = value; }
        }

        private Hashtable _relativeSourceFiles = new Hashtable();
        private Hashtable _relativeResourceFiles = new Hashtable();

        private int _added = 0;
        private int _removed = 0;

        private static string RelativePath(string baseDir, string rootedName)
        {
            if (rootedName.ToLower().StartsWith(baseDir.ToLower()))
            {
                string s = rootedName.Substring(baseDir.Length);

                return s;
            }
            throw new Exception("Path " + rootedName + " is not within " + baseDir);
        }

        protected override void ExecuteTask() {
            string baseDir = SourceFiles.BaseDirectory.FullName;
            if (!baseDir.EndsWith("\\"))
                baseDir = baseDir + "\\";

            foreach (string s in SourceFiles.FileNames)
            {
                _relativeSourceFiles[RelativePath(baseDir, s)] = s;
            }

            foreach (string projectFile in ProjectFiles.FileNames)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(projectFile);
                _added = 0;
                _removed = 0;

                if (doc.SelectSingleNode("//Files/Include") != null)
                {
                    Log(Level.Verbose, "Visual Studio 2002/2003-style project.");
                    ProcessOldProject(doc);
                }
                else
                {
                    Log(Level.Verbose, "MSBuild-style project.");
                    ProcessMSBuildProject(doc);
                }
                if (_added + _removed > 0)
                {
                    Log(Level.Info, "Project: {0} Added: {1} Removed: {2}", projectFile, _added, _removed);
                    doc.Save(projectFile);
                }
            }
        }

        private void ProcessOldProject(XmlDocument doc)
        {
            SyncOldProjectItem(doc, "Compile", "Code", _relativeSourceFiles);
            SyncOldProjectItem(doc, "EmbeddedResource", null, _relativeResourceFiles);
        }

        private void SyncOldProjectItem(XmlDocument doc, string buildAction, string subType, Hashtable directoryFiles)
        {
            Hashtable projectSourceFiles = new Hashtable();

            foreach (XmlElement el in doc.SelectNodes("//File[@BuildAction='" + buildAction + "']"))
            {
                string name = el.GetAttribute("RelPath");
                projectSourceFiles[name] = el;
            }

            XmlElement filesInclude = (XmlElement)doc.SelectSingleNode("//Files/Include");

            foreach (string s in projectSourceFiles.Keys)
            {
                if (!directoryFiles.Contains(s))
                {
                    XmlElement el = (XmlElement)projectSourceFiles[s];
                    Log(Level.Verbose, "File {0} not found in directory. Removing.", s);
                    el.ParentNode.RemoveChild(el);
                    _removed++;
                }
            }
            foreach (string s in directoryFiles.Keys)
            {
                if (!projectSourceFiles.Contains(s))
                {
                    Log(Level.Verbose, "File {0} not found in project. Adding.", s);
                    XmlElement el = doc.CreateElement("File");
                    el.SetAttribute("RelPath", s);
                    if (subType != null)
                        el.SetAttribute("SubType", subType);
                    el.SetAttribute("BuildAction", buildAction);
                    filesInclude.AppendChild(el);
                    _added++;
                }
            }
        }

        private void ProcessMSBuildProject(XmlDocument doc)
        {
            SyncProjectItem(doc, "Compile", "Code", _relativeSourceFiles);
            SyncProjectItem(doc, "EmbeddedResource", null, _relativeResourceFiles);
        }

        private void SyncProjectItem(XmlDocument doc, string type, string subType, Hashtable directoryFiles)
        {
            string msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";

            Hashtable projectSourceFiles = new Hashtable();
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("msb", msbuildNamespace);

            XmlElement itemGroup = null;

            foreach (XmlElement el in doc.SelectNodes("//msb:" + type, mgr))
            {
                string name = el.GetAttribute("Include");
                projectSourceFiles[name] = el;

                if (itemGroup == null)
                    itemGroup = (XmlElement)el.ParentNode;
            }

            if (itemGroup == null)
            {
                if (directoryFiles.Count > 0)
                    return;
                XmlNode importNode = doc.SelectSingleNode("//msb:Import", mgr);
                if (importNode == null)
                    throw new Exception("No <Import> node in project.");

                itemGroup = doc.CreateElement("ItemGroup", msbuildNamespace);

                doc.DocumentElement.InsertBefore(itemGroup, importNode);
            }

            foreach (string s in projectSourceFiles.Keys)
            {
                if (!directoryFiles.Contains(s))
                {
                    XmlElement el = (XmlElement)projectSourceFiles[s];
                    Log(Level.Verbose, "File {0} not found in directory. Removing.", s);
                    el.ParentNode.RemoveChild(el);
                    _removed++;
                }
            }
            foreach (string s in directoryFiles.Keys)
            {
                if (!projectSourceFiles.Contains(s))
                {
                    Log(Level.Verbose, "File {0} not found in project. Adding.", s);
                    XmlElement el = doc.CreateElement(type, msbuildNamespace);
                    el.SetAttribute("Include", s);
                    itemGroup.AppendChild(el);
                    _added++;
                }
            }
        }
    }
}
