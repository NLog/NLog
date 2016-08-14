using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog.Internal.Fakeables;
using NLog.LayoutRenderers;
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

        /// <summary>
        /// not null when <see cref="_filePathKind"/> == <c>false</c>
        /// </summary>
        private string _baseDir;

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

            if (_filePathKind == FilePathKind.Relative)
            {
                _baseDir = AppDomainWrapper.CurrentDomain.BaseDirectory;
            }

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
            if (_filePathKind == FilePathKind.Absolute)
            {
                return rendered;
            }
            else if (_filePathKind == FilePathKind.Relative)
            {
                return Path.Combine(_baseDir, rendered);
                //use basedir, faster than Path.GetFullPath
            }
            else
            {
                //unknown, use slow method
                return Path.GetFullPath(rendered);
            }
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
