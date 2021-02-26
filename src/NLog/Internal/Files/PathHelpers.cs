// 
// Copyright (c) 2004-2020 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

using System.IO;

namespace NLog.Internal
{
    internal static class PathHelpers
    {
        /// <summary>
        /// Combine paths
        /// </summary>
        /// <param name="path">basepath, not null</param>
        /// <param name="dir">optional dir</param>
        /// <param name="file">optional file</param>
        /// <returns></returns>
        internal static string CombinePaths(string path, string dir, string file)
        {
            if (dir != null)
            {
                path = Path.Combine(path, dir);
            }

            if (file != null)
            {
                path = Path.Combine(path, file);
            }
            return path;
        }

        /// <summary>
        /// Cached directory separator char array to avoid memory allocation on each method call.
        /// </summary>
        private static readonly char[] DirectorySeparatorChars = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

        /// <summary>
        /// Trims directory separators from the path
        /// </summary>
        /// <param name="path">path, could be null</param>
        /// <returns>never null</returns>
        public static string TrimDirectorySeparators(string path)
        {
            var newpath = path?.TrimEnd(DirectorySeparatorChars) ?? string.Empty;
            if (newpath.EndsWith(":", System.StringComparison.OrdinalIgnoreCase))
                return path;    // Support root-path on Windows
            else if (string.IsNullOrEmpty(newpath) && !string.IsNullOrEmpty(path))
                return path;    // Support root-path on Linux
            else
                return newpath;
        }

        public static bool IsTempDir(string directory, string tempDir)
        {
            tempDir = TrimDirectorySeparators(tempDir);
            if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(tempDir))
                return false;

            var fullpath = Path.GetFullPath(directory);
            if (string.IsNullOrEmpty(fullpath))
                return false;

            if (fullpath.StartsWith(tempDir, System.StringComparison.OrdinalIgnoreCase))
                return true;

            if (tempDir.StartsWith("/tmp") && directory.StartsWith("/var/tmp/"))
                return true;    // Microsoft has made a funny joke on Linux. Path.GetTempPath() uses /tmp/ as fallback, but single-publish uses /var/tmp/ as first fallback

            return false;
        }
    }
}
