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

using System;
using System.IO;
using NLog.Internal.Fakeables;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Internal
{
    /// <summary>
    /// A layout that represents a filePath. 
    /// </summary>
    internal class FilePathLayout
    {
        private Layout _layout;

        private FilePathKind _filePathKind;

#if !SILVERLIGHT
        /// <summary>
        /// not null when <see cref="_filePathKind"/> == <c>false</c>
        /// </summary>
        private string _baseDir;
#endif

        /// <summary>
        /// non null is fixed,
        /// </summary>
        private string cleanedFixedResult;

        private bool _cleanupInvalidChars;

        //TODO onInit maken
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public FilePathLayout(Layout layout, bool cleanupInvalidChars, FilePathKind filePathKind)
        {
            _layout = layout;
            _filePathKind = filePathKind;
            _cleanupInvalidChars = cleanupInvalidChars;

            //do we have to the the layout?
            if (cleanupInvalidChars || _filePathKind == FilePathKind.Unknown)
            {
                //check if fixed 
                var pathLayout2 = layout as SimpleLayout;
                if (pathLayout2 != null)
                {
                    var isFixedText = pathLayout2.IsFixedText;
                    if (isFixedText)
                    {
                        cleanedFixedResult = pathLayout2.FixedText;
                        if (cleanupInvalidChars)
                        {
                            //clean first
                            cleanedFixedResult = FileTarget.CleanupInvalidFileNameChars2(cleanedFixedResult);
                        }
                    }

                    //detect absolute
                    if (_filePathKind == FilePathKind.Unknown)
                    {
                        _filePathKind = DetectFilePathKind(pathLayout2, !isFixedText);
                    }
                }
                else
                {
                    _filePathKind = FilePathKind.Unknown;
                }
            }
#if !SILVERLIGHT

            if (_filePathKind == FilePathKind.Relative)
            {
                _baseDir = AppDomainWrapper.CurrentDomain.BaseDirectory;
            }
#endif

        }

        public Layout GetLayout()
        {
            return _layout;
        }

        #region Implementation of IRenderable

        /// <summary>
        /// Render, as cleaned if requested.
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <returns>String representation of a layout.</returns>
        private string Render(LogEventInfo logEvent)
        {
            if (cleanedFixedResult != null)
            {
                return cleanedFixedResult;
            }
            if (_layout == null)
            {
                return null;
            }

            var result = _layout.Render(logEvent);
            if (_cleanupInvalidChars)
            {
                return FileTarget.CleanupInvalidFileNameChars2(result);
            }
            return result;
        }

        public string GetAsAbsolutePath(LogEventInfo logEvent)
        {
            var rendered = Render(logEvent);
            if (string.IsNullOrEmpty(rendered))
            {
                return rendered;
            }

            if (_filePathKind == FilePathKind.Absolute)
            {
                return rendered;
            }
#if !SILVERLIGHT
            if (_filePathKind == FilePathKind.Relative)
            {
                return Path.Combine(_baseDir, rendered);
                //use basedir, faster than Path.GetFullPath
            }
#endif
            //unknown, use slow method
            return Path.GetFullPath(rendered);

        }

        #endregion


        /// <summary>
        /// Is this (templated/invalid) path an absolute, relative or unknown?
        /// </summary>
        /// <returns> <c>true</c> for absolute, <c>false</c> for relative, <c>null</c> for unknown </returns>
        internal static FilePathKind DetectFilePathKind(Layout pathLayout)
        {
            var pathLayout2 = pathLayout as SimpleLayout;
            if (pathLayout2 == null)
            {
                return FilePathKind.Unknown;
            }

            var containsLayoutRenderers = !pathLayout2.IsFixedText;

            return DetectFilePathKind(pathLayout2, containsLayoutRenderers);
        }

        private static FilePathKind DetectFilePathKind(SimpleLayout pathLayout2, bool containsLayoutRenderers)
        {
            var path = pathLayout2.OriginalText;

            if (path != null)
            {
                path = path.TrimStart();

                int length = path.Length;
                if (length >= 1)
                {
                    var firstChar = path[0];
                    if (firstChar == Path.DirectorySeparatorChar || firstChar == Path.AltDirectorySeparatorChar)
                        return FilePathKind.Absolute;
                    if (firstChar == '.') //. and ..
                    {
                        return FilePathKind.Relative;
                    }


                    if (length >= 2)
                    {
                        var secondChar = path[1];
                        if (secondChar == Path.VolumeSeparatorChar)
                            return FilePathKind.Absolute;

                        //starts with template-character, and not ${basedir}
                        if (containsLayoutRenderers && firstChar == '$' && secondChar == '{')
                        {
                            if (path.StartsWith("${basedir}", StringComparison.OrdinalIgnoreCase))
                            {
                                return FilePathKind.Absolute;
                            }
                            //unknown what this will render
                            return FilePathKind.Unknown;
                        }
                    }

                    //not a layout renderer, but text
                    return FilePathKind.Relative;
                }
            }
            return FilePathKind.Unknown;
        }
    }
}
