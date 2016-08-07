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
    internal class FilePathLayout : IRenderable
    {
        private Layout _layout;

        private bool? _isAbsolute;

        /// <summary>
        /// not null when <see cref="_isAbsolute"/> == <c>false</c>
        /// </summary>
        private string _baseDir;

        /// <summary>
        /// non null is fixed,
        /// </summary>
        private string cleanedFixedResult;

        private bool _cleanupInvalidChars;

        //TODO onInit maken
        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public FilePathLayout(Layout layout, bool cleanupInvalidChars, bool? isAbsoluteAlready)
        {
            _layout = layout;

            _isAbsolute = isAbsoluteAlready;

            _cleanupInvalidChars = cleanupInvalidChars;

            if (cleanupInvalidChars || isAbsoluteAlready == null)
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
                    if (isAbsoluteAlready == null)
                    {
                        _isAbsolute = IsAbsolutePath(pathLayout2, !isFixedText);
                    }
                }
                else
                {
                    _isAbsolute = null;
                }
            }

            if (_isAbsolute == false)
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
        public string Render(LogEventInfo logEvent)
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

        public string RenderAsAbsolutePath(LogEventInfo logEvent)
        {
            var rendered = Render(logEvent);
            if (_isAbsolute == true)
            {
                return rendered;
            }
            else if (_isAbsolute == false)
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
        internal static bool? IsAbsolutePath(Layout pathLayout)
        {
            var pathLayout2 = pathLayout as SimpleLayout;
            if (pathLayout2 == null)
            {
                return null;
            }

            var containsLayoutRenderers = !pathLayout2.IsFixedText;

            return IsAbsolutePath(pathLayout2, containsLayoutRenderers);
        }

        private static bool? IsAbsolutePath(SimpleLayout pathLayout2, bool containsLayoutRenderers)
        {
            var path = pathLayout2.OriginalText;
            const bool absolute = true;
            const bool relative = false;

            if (path != null)
            {
                path = path.TrimStart();

                int length = path.Length;
                if (length >= 1)
                {
                    var firstChar = path[0];
                    if (firstChar == Path.DirectorySeparatorChar || firstChar == Path.AltDirectorySeparatorChar)
                        return absolute;
                    if (firstChar == '.') //. and ..
                    {
                        return relative;
                    }


                    if (length >= 2)
                    {
                        var secondChar = path[1];
                        if (secondChar == Path.VolumeSeparatorChar)
                            return absolute;

                        //starts with template-character, and not ${basedir}
                        if (containsLayoutRenderers && firstChar == '$' && secondChar == '{')
                        {
                            if (path.StartsWith("${basedir}", StringComparison.OrdinalIgnoreCase))
                            {
                                return absolute;
                            }
                            //unknown what this will render
                            return null;
                        }
                    }

                    //not a layout renderer, but text
                    return relative;
                }
            }
            return null;
        }
    }
}
