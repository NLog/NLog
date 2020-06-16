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

using System;

namespace NLog.Internal.FileAppenders
{
    internal interface IFileAppenderCache : IDisposable
    {
        /// <summary>
        /// Gets the parameters which will be used for creating a file.
        /// </summary>
        ICreateFileParameters CreateFileParameters { get; }

        /// <summary>
        /// Gets the file appender factory used by all the appenders in this list.
        /// </summary>
        IFileAppenderFactory Factory { get; }

        /// <summary>
        /// Gets the number of appenders which the list can hold.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Subscribe to background monitoring of active file appenders
        /// </summary>
        event EventHandler CheckCloseAppenders;

        /// <summary>
        /// It allocates the first slot in the list when the file name does not already in the list and clean up any
        /// unused slots.
        /// </summary>
        /// <param name="fileName">File name associated with a single appender.</param>
        /// <returns>The allocated appender.</returns>
        /// <exception cref="NullReferenceException">
        /// Thrown when <see cref="M:AllocateAppender"/> is called on an <c>Empty</c><see cref="FileAppenderCache"/> instance.
        /// </exception>
        BaseFileAppender AllocateAppender(string fileName);

        /// <summary>
        /// Close all the allocated appenders. 
        /// </summary>
        void CloseAppenders(string reason);

        /// <summary>
        /// Close the allocated appenders initialized before the supplied time.
        /// </summary>
        /// <param name="expireTime">The time which prior the appenders considered expired</param>
        void CloseAppenders(DateTime expireTime);

        /// <summary>
        /// Flush all the allocated appenders. 
        /// </summary>
        void FlushAppenders();

        DateTime? GetFileCreationTimeSource(string filePath, DateTime? fallbackTimeSource = null);

        /// <summary>
        /// File Archive Logic uses the File-Creation-TimeStamp to detect if time to archive, and the File-LastWrite-Timestamp to name the archive-file.
        /// </summary>
        /// <remarks>
        /// NLog always closes all relevant appenders during archive operation, so no need to lookup file-appender
        /// </remarks>
        DateTime? GetFileLastWriteTimeUtc(string filePath);

        long? GetFileLength(string filePath);

        /// <summary>
        /// Closes the specified appender and removes it from the list. 
        /// </summary>
        /// <param name="filePath">File name of the appender to be closed.</param>
        /// <returns>File Appender that matched the filePath (null if none found)</returns>
        BaseFileAppender InvalidateAppender(string filePath);

#if !NETSTANDARD1_3
        
        /// <summary>
        /// The archive file path pattern that is used to detect when archiving occurs.
        /// </summary>
        string ArchiveFilePatternToWatch { get; set; }

        /// <summary>
        /// Invalidates appenders for all files that were archived.
        /// </summary>
        void InvalidateAppendersForArchivedFiles();

#endif
    }
}