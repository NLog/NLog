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

#if !NETSTANDARD1_3 && !NETSTANDARD1_5

namespace NLog.LayoutRenderers
{
    using System;
    using System.Text;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// System special folder path (includes My Documents, My Music, Program Files, Desktop, and more).
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Special-Folder-Layout-Renderer">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Special-Folder-Layout-Renderer">Documentation on NLog Wiki</seealso>
    [LayoutRenderer("specialfolder")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    public class SpecialFolderLayoutRenderer : LayoutRenderer
    {
        /// <summary>
        /// Gets or sets the system special folder to use.
        /// </summary>
        /// <remarks>
        /// Full list of options is available at <a href="https://docs.microsoft.com/en-us/dotnet/api/system.environment.specialfolder">MSDN</a>.
        /// The most common ones are:
        /// <ul>
        /// <li><b>CommonApplicationData</b> - application data for all users.</li>
        /// <li><b>ApplicationData</b> - roaming application data for current user.</li>
        /// <li><b>LocalApplicationData</b> - non roaming application data for current user</li>
        /// <li><b>UserProfile</b> - Profile folder for current user</li>
        /// <li><b>DesktopDirectory</b> - Desktop-directory for current user</li>
        /// <li><b>MyDocuments</b> - My Documents-directory for current user</li>
        /// <li><b>System</b> - System directory</li>
        /// </ul>
        /// </remarks>
        /// <docgen category='Layout Options' order='10' />
        [DefaultParameter]
        public Environment.SpecialFolder Folder { get; set; }

        /// <summary>
        /// Gets or sets the name of the file to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string File { get; set; }

        /// <summary>
        /// Gets or sets the name of the directory to be Path.Combine()'d with the directory name.
        /// </summary>
        /// <docgen category='Advanced Options' order='10' />
        public string Dir { get; set; }

        /// <inheritdoc/>
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            string basePath = GetFolderPath(Folder);
            var path = PathHelpers.CombinePaths(basePath, Dir, File);
            builder.Append(path);
        }

        internal static string GetFolderPath(Environment.SpecialFolder folder)
        {
            try
            {
                var folderPath = Environment.GetFolderPath(folder);
                if (!string.IsNullOrEmpty(folderPath))
                    return folderPath;
            }
            catch
            {
                var folderPath = GetFolderPathFromEnvironment(folder);
                if (!string.IsNullOrEmpty(folderPath))
                    return folderPath;

                throw;
            }

            return GetFolderPathFromEnvironment(folder);
        }

        private static string GetFolderPathFromEnvironment(Environment.SpecialFolder folder)
        {
            try
            {
                var variableName = GetFolderWindowsEnvironmentVariable(folder);
                if (string.IsNullOrEmpty(variableName))
                    return string.Empty;
                if (!PlatformDetector.IsWin32)
                    return string.Empty;

                // Fallback for Windows Nano: https://github.com/dotnet/runtime/issues/21430
                var folderPath = Environment.GetEnvironmentVariable(variableName);
                return folderPath ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string GetFolderWindowsEnvironmentVariable(Environment.SpecialFolder folder)
        {
            switch (folder)
            {
                case Environment.SpecialFolder.CommonApplicationData: return "COMMONAPPDATA";   // Default user
                case Environment.SpecialFolder.LocalApplicationData: return "LOCALAPPDATA";     // Current user
                case Environment.SpecialFolder.ApplicationData: return "APPDATA";               // Current user
#if !NET35
                case Environment.SpecialFolder.UserProfile: return "USERPROFILE";               // Current user
#endif
                default: return string.Empty;
            }
        }
    }
}

#endif