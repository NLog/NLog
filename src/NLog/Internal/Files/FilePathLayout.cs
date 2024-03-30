// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.Collections.Generic;
using System.IO;
using NLog.Common;
using NLog.Layouts;
using NLog.Targets;

namespace NLog.Internal
{
    /// <summary>
    /// A layout that represents a filePath. 
    /// </summary>
    internal sealed class FilePathLayout : IRenderable
    {
        /// <summary>
        /// Cached directory separator char array to avoid memory allocation on each method call.
        /// </summary>
        private static readonly char[] DirectorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Cached invalid file names char array to avoid memory allocation every time Path.GetInvalidFileNameChars() is called.
        /// </summary>
        private static readonly HashSet<char> InvalidFileNameChars = new HashSet<char>(Path.GetInvalidFileNameChars());

        private readonly Layout _layout;

        private readonly FilePathKind _filePathKind;

        /// <summary>
        /// not null when <see cref="_filePathKind"/> == <c>false</c>
        /// </summary>
        private readonly string _baseDir;

        /// <summary>
        /// non null is fixed,
        /// </summary>
        private readonly string _cleanedFixedResult;

        private readonly bool _cleanupInvalidChars;

        /// <summary>
        /// <see cref="_cachedPrevRawFileName"/> is the cache-key, and when newly rendered filename matches the cache-key,
        /// then it reuses the cleaned cache-value <see cref="_cachedPrevCleanFileName"/>.
        /// </summary>
        private string _cachedPrevRawFileName;
        /// <summary>
        /// <see cref="_cachedPrevCleanFileName"/> is the cache-value that is reused, when the newly rendered filename
        /// matches the cache-key <see cref="_cachedPrevRawFileName"/>
        /// </summary>
        private string _cachedPrevCleanFileName;

        public bool IsFixedFilePath => _cleanedFixedResult != null;

        /// <summary>Initializes a new instance of the <see cref="FilePathLayout" /> class.</summary>
        public FilePathLayout(Layout layout, bool cleanupInvalidChars, FilePathKind filePathKind)
        {
            _layout = layout ?? new SimpleLayout(null);
            _cleanupInvalidChars = cleanupInvalidChars;
            _filePathKind = filePathKind == FilePathKind.Unknown ? DetectFilePathKind(_layout as SimpleLayout) : filePathKind;
            _cleanedFixedResult = (_layout as SimpleLayout)?.FixedText;

            if (_filePathKind == FilePathKind.Relative)
            {
                _baseDir = LogFactory.DefaultAppEnvironment.AppDomainBaseDirectory;
                InternalLogger.Debug("FileTarget FilePathLayout with FilePathKind.Relative using AppDomain.BaseDirectory: {0}", _baseDir);
            }
           
            if (_cleanedFixedResult != null)
            {
                if (!StringHelpers.IsNullOrWhiteSpace(_cleanedFixedResult))
                {
                    _cleanedFixedResult = GetCleanFileName(_cleanedFixedResult);
                    _filePathKind = FilePathKind.Absolute;
                }
                else
                {
                    _filePathKind = FilePathKind.Unknown;
                }
            }

            if (layout is SimpleLayout simpleLayout && simpleLayout.OriginalText?.IndexOfAny(new char[] { '\a', '\b', '\f', '\n', '\r', '\t', '\v' }) >= 0)
            {
                if (_cleanedFixedResult is null)
                    InternalLogger.Warn("FileTarget FilePathLayout contains unexpected escape characters (Maybe change to forward-slash): {0}", simpleLayout.OriginalText);
                else
                    InternalLogger.Warn("FileTarget FilePathLayout contains unexpected escape characters (Maybe change to forward-slash): {0}", _cleanedFixedResult);
            }
            else if (_cleanedFixedResult != null)
            {
                if (DetectFilePathKind(_cleanedFixedResult) != FilePathKind.Absolute && _cleanedFixedResult.IndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }) < 0)
                {
                    InternalLogger.Warn("FileTarget FilePathLayout not recognized as absolute path (Maybe change to forward-slash): {0}", _cleanedFixedResult);
                }
            }
        }

        public Layout GetLayout()
        {
            return _layout;
        }

        /// <summary>
        /// Render the raw filename from Layout
        /// </summary>
        /// <param name="logEvent">The log event.</param>
        /// <param name="reusableBuilder">StringBuilder to minimize allocations [optional].</param>
        /// <returns>String representation of a layout.</returns>
        private string GetRenderedFileName(LogEventInfo logEvent, System.Text.StringBuilder reusableBuilder = null)
        {
            if (reusableBuilder is null)
            {
                return _layout.Render(logEvent);
            }
            else
            {
                if (!_layout.ThreadAgnostic || _layout.ThreadAgnosticImmutable)
                {
                    object cachedResult;
                    if (logEvent.TryGetCachedLayoutValue(_layout, out cachedResult))
                    {
                        return cachedResult?.ToString() ?? string.Empty;
                    }
                }

                _layout.Render(logEvent, reusableBuilder);

                if (_cachedPrevRawFileName != null && reusableBuilder.EqualTo(_cachedPrevRawFileName))
                {
                    // If old filename matches the newly rendered, then no need to call StringBuilder.ToString()
                    return _cachedPrevRawFileName;
                }

                _cachedPrevRawFileName = reusableBuilder.ToString();
                _cachedPrevCleanFileName = null;
                return _cachedPrevRawFileName;
            }
        }

        /// <summary>
        /// Convert the raw filename to a correct filename
        /// </summary>
        /// <param name="rawFileName">The filename generated by Layout.</param>
        /// <returns>String representation of a correct filename.</returns>
        private string GetCleanFileName(string rawFileName)
        {
            var cleanFileName = rawFileName;
            if (_cleanupInvalidChars)
            {
                cleanFileName = CleanupInvalidFilePath(rawFileName);
            }

            if (_filePathKind == FilePathKind.Absolute)
            {
                return cleanFileName;
            }

            if (_filePathKind == FilePathKind.Relative && _baseDir != null)
            {
                //use basedir, faster than Path.GetFullPath
                cleanFileName = Path.Combine(_baseDir, cleanFileName);
                return cleanFileName;
            }

            //unknown, use slow method
            cleanFileName = Path.GetFullPath(cleanFileName);
            return cleanFileName;
        }

        public string Render(LogEventInfo logEvent)
        {
            return RenderWithBuilder(logEvent);
        }

        internal string RenderWithBuilder(LogEventInfo logEvent, System.Text.StringBuilder reusableBuilder = null)
        {
            if (_cleanedFixedResult != null)
                return _cleanedFixedResult; // Always absolute path

            var rawFileName = GetRenderedFileName(logEvent, reusableBuilder);
            if (string.IsNullOrEmpty(rawFileName))
                return rawFileName;

            if (_filePathKind == FilePathKind.Absolute && !_cleanupInvalidChars)
                return rawFileName; // Skip clean filename string-allocation

            if (string.Equals(_cachedPrevRawFileName, rawFileName, StringComparison.Ordinal) && _cachedPrevCleanFileName != null)
                return _cachedPrevCleanFileName;    // Cache Hit, reuse clean filename string-allocation

            var cleanFileName = GetCleanFileName(rawFileName);
            _cachedPrevCleanFileName = cleanFileName;
            _cachedPrevRawFileName = rawFileName;
            return cleanFileName;
        }

        /// <summary>
        /// Is this (templated/invalid) path an absolute, relative or unknown?
        /// </summary>
        internal static FilePathKind DetectFilePathKind(SimpleLayout pathLayout)
        {
            if (pathLayout is null)
                return FilePathKind.Unknown;

            var isFixedText = pathLayout.IsFixedText;

            //nb: ${basedir} has already been rewritten in the SimpleLayout.compile
            var path = isFixedText ? pathLayout.FixedText : pathLayout.Text;
            return DetectFilePathKind(path, isFixedText);
        }

        internal static FilePathKind DetectFilePathKind(string path, bool isFixedText = true)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = path.TrimStart(ArrayHelper.Empty<char>());

                int length = path.Length;
                if (length >= 1)
                {
                    var firstChar = path[0];
                    if (IsAbsoluteStartChar(firstChar))
                        return FilePathKind.Absolute;

                    if (firstChar == '.') //. and ..
                    {
                        return FilePathKind.Relative;
                    }

                    if (length >= 2)
                    {
                        var secondChar = path[1];
                        //on unix VolumeSeparatorChar == DirectorySeparatorChar
                        if (Path.VolumeSeparatorChar != Path.DirectorySeparatorChar && secondChar == Path.VolumeSeparatorChar)
                            return FilePathKind.Absolute;
                    }

                    if (IsLayoutRenderer(path, isFixedText))
                    {
                        //if first part is a layout, then unknown
                        return FilePathKind.Unknown;
                    }

                    //not a layout renderer, but text
                    return FilePathKind.Relative;
                }
            }
            return FilePathKind.Unknown;
        }

        private static bool IsLayoutRenderer(string path, bool isFixedText)
        {
            return !isFixedText && path.StartsWith("${", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAbsoluteStartChar(char firstChar)
        {
            return firstChar == Path.DirectorySeparatorChar || firstChar == Path.AltDirectorySeparatorChar;
        }

        private static string CleanupInvalidFilePath(string filePath)
        {
            if (StringHelpers.IsNullOrWhiteSpace(filePath))
            {
                return filePath;
            }

            var lastDirSeparator = filePath.LastIndexOfAny(DirectorySeparatorChars);

            char[] fileNameChars = null;

            for (int i = lastDirSeparator + 1; i < filePath.Length; i++)
            {
                if (InvalidFileNameChars.Contains(filePath[i]))
                {
                    //delay char[] creation until first invalid char
                    //is found to avoid memory allocation.
                    if (fileNameChars is null)
                    {
                        fileNameChars = filePath.Substring(lastDirSeparator + 1).ToCharArray();
                    }
                    fileNameChars[i - (lastDirSeparator + 1)] = '_';
                }
            }

            //only if an invalid char was replaced do we create a new string.
            if (fileNameChars != null)
            {
                //keep the / in the dirname, because dirname could be c:/ and combine of c: and file name won't work well.
                var dirName = lastDirSeparator > 0 ? filePath.Substring(0, lastDirSeparator + 1) : string.Empty;
                string fileName = new string(fileNameChars);
                return Path.Combine(dirName, fileName);
            }

            return filePath;
        }
    }
}
