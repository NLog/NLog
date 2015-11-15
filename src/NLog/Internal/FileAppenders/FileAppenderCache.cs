﻿// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
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

namespace NLog.Internal.FileAppenders
{
    using System;

    /// <summary>
    /// Maintains a collection of file appenders usually associated with file targets.
    /// </summary>
    internal sealed class FileAppenderCache
    {
        private BaseFileAppender[] appenders;

        /// <summary>
        /// Initializes a new "empty" instance of the <see cref="FileAppenderCache"/> class with zero size and empty
        /// list of appenders.
        /// </summary>
        public static readonly FileAppenderCache Empty = new FileAppenderCache();

        /// <summary>
        /// Initializes a new "empty" instance of the <see cref="FileAppenderCache"/> class with zero size and empty
        /// list of appenders.
        /// </summary>
        private FileAppenderCache()
        {
            Size = 0;
            Factory = null;
            CreateFileParameters = null;

            appenders = new BaseFileAppender[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAppenderCache"/> class.
        /// </summary>
        /// <remarks>
        /// The size of the list should be positive. No validations are performed during initialisation as it is an
        /// intenal class.
        /// </remarks>
        /// <param name="size">Total number of appenders allowed in list.</param>
        /// <param name="appenderFactory">Factory used to create each appender.</param>
        /// <param name="createFileParams">Parameters used for creating a file.</param>
        public FileAppenderCache(int size, IFileAppenderFactory appenderFactory, ICreateFileParameters createFileParams)
        {
            Size = size;
            Factory = appenderFactory;
            CreateFileParameters = createFileParams;

            appenders = new BaseFileAppender[Size];
        }

        /// <summary>
        /// Gets the parameters which will be used for creating a file.
        /// </summary>
        public ICreateFileParameters CreateFileParameters { get; private set; }

        /// <summary>
        /// Gets the file appender factory used by all the appenders in this list.
        /// </summary>
        public IFileAppenderFactory Factory { get; private set; }

        /// <summary>
        /// Gets the number of appenders which the list can hold.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// It allocates the first slot in the list when the file name does not already in the list and clean up any
        /// unused slots.
        /// </summary>
        /// <param name="fileName">File name associated with a single appender.</param>
        /// <returns>The allocated appender.</returns>
        /// <exception cref="NullReferenceException">
        /// Thrown when <see cref="M:AllocateAppender"/> is called on an <c>Empty</c><see cref="FileAppenderCache"/> instance.
        /// </exception>
        public BaseFileAppender AllocateAppender(string fileName)
        {
            //
            // BaseFileAppender.Write is the most expensive operation here
            // so the in-memory data structure doesn't have to be 
            // very sophisticated. It's a table-based LRU, where we move 
            // the used element to become the first one.
            // The number of items is usually very limited so the 
            // performance should be equivalent to the one of the hashtable.
            //

            BaseFileAppender appenderToWrite = null;
            int freeSpot = appenders.Length - 1;

            for (int i = 0; i < appenders.Length; ++i)
            {
                // Use empty slot in recent appender list, if there is one.
                if (appenders[i] == null)
                {
                    freeSpot = i;
                    break;
                }

                if (appenders[i].FileName == fileName)
                {
                    // found it, move it to the first place on the list
                    // (MRU)

                    // file open has a chance of failure
                    // if it fails in the constructor, we won't modify any data structures
                    BaseFileAppender app = appenders[i];
                    for (int j = i; j > 0; --j)
                    {
                        appenders[j] = appenders[j - 1];
                    }

                    appenders[0] = app;
                    appenderToWrite = app;
                    break;
                }
            }

            if (appenderToWrite == null)
            {
                BaseFileAppender newAppender = Factory.Open(fileName, CreateFileParameters);

                if (appenders[freeSpot] != null)
                {
                    appenders[freeSpot].Close();
                    appenders[freeSpot] = null;
                }

                for (int j = freeSpot; j > 0; --j)
                {
                    appenders[j] = appenders[j - 1];
                }

                appenders[0] = newAppender;
                appenderToWrite = newAppender;
            }

            return appenderToWrite;
        }

        /// <summary>
        /// Close all the allocated appenders. 
        /// </summary>
        public void CloseAppenders()
        {
            if (appenders != null)
            {
                for (int i = 0; i < appenders.Length; ++i)
                {
                    if (appenders[i] == null)
                    {
                        break;
                    }

                    appenders[i].Close();
                    appenders[i] = null;
                }
            }
        }

        /// <summary>
        /// Close the allocated appenders initialised before the supplied time.
        /// </summary>
        /// <param name="expireTime">The time which prior the appenders considered expired</param>
        public void CloseAppenders(DateTime expireTime)
        {
            for (int i = 0; i < this.appenders.Length; ++i)
            {
                if (this.appenders[i] == null)
                {
                    break;
                }

                if (this.appenders[i].OpenTime < expireTime)
                {
                    for (int j = i; j < this.appenders.Length; ++j)
                    {
                        if (this.appenders[j] == null)
                        {
                            break;
                        }

                        this.appenders[j].Close();
                        this.appenders[j] = null;
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Fluch all the allocated appenders. 
        /// </summary>
        public void FlushAppenders()
        {
            foreach (BaseFileAppender appender in appenders)
            {
                if (appender == null)
                {
                    break;
                }

                appender.Flush();
            }
        }
        
        /// <summary>
        /// Gets the file info for a particular appender.
        /// </summary>
        /// <param name="fileName">The file name associated with a particular appender.</param>
        /// <param name="lastWriteTime">The last file write time. The value must be of UTC kind.</param>
        /// <param name="fileLength">Length of the file.</param>
        /// <returns><see langword="true"/> when the operation succeeded; <see langword="false"/> otherwise.</returns>
        public bool GetFileInfo(string fileName, out DateTime lastWriteTime, out long fileLength)
        {
            foreach (BaseFileAppender appender in appenders)
            {
                if (appender == null)
                {
                    break;
                }

                if (appender.FileName == fileName)
                {
                    appender.GetFileInfo(out lastWriteTime, out fileLength);
                    return true;
                }
            }

            // Return default values.
            fileLength = -1;
            lastWriteTime = DateTime.MinValue;
            return false;
        }

        /// <summary>
        /// Closes the specified appender and removes it from the list. 
        /// </summary>
        /// <param name="fileName">File name of the appender to be closed.</param>
        public void InvalidateAppender(string fileName)
        {
            for (int i = 0; i < appenders.Length; ++i)
            {
                if (appenders[i] == null)
                {
                    break;
                }

                if (appenders[i].FileName == fileName)
                {
                    appenders[i].Close();
                    for (int j = i; j < appenders.Length - 1; ++j)
                    {
                        appenders[j] = appenders[j + 1];
                    }

                    appenders[appenders.Length - 1] = null;
                    break;
                }
            }
        }
    }
}
