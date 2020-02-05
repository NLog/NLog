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

#if SILVERLIGHT
#define TLS_WORKAROUND
#endif

namespace NLog.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Helper for dealing with thread-local storage.
    /// </summary>
    internal static class ThreadLocalStorageHelper
    {
#if TLS_WORKAROUND
        private static int nextSlotNumber;
        private static WeakThreadReferenceWithData threadData;
#endif

        /// <summary>
        /// Allocates the data slot for storing thread-local information.
        /// </summary>
        /// <returns>Allocated slot key.</returns>
        public static object AllocateDataSlot()
        {
#if TLS_WORKAROUND
            return Interlocked.Increment(ref nextSlotNumber);
#elif NETSTANDARD || NET4_6
            return new System.Threading.ThreadLocal<object>();
#else
            return Thread.AllocateDataSlot();
#endif
        }

        /// <summary>
        /// Gets the data for a slot in thread-local storage.
        /// </summary>
        /// <typeparam name="T">Type of the data.</typeparam>
        /// <param name="slot">The slot to get data for.</param>
        /// <param name="create">Automatically create the object if it doesn't exist.</param>
        /// <returns>
        /// Slot data (will create T if null).
        /// </returns>
        public static T GetDataForSlot<T>(object slot, bool create = true)
            where T : class, new()
        {
#if TLS_WORKAROUND
            IDictionary<int, object> dict = GetThreadDataDictionary(Thread.CurrentThread);
            int slotNumber = (int)slot;
            object v;
            if (!dict.TryGetValue(slotNumber, out v))
            {
                if (!create)
                    return null;

                v = new T();
                dict.Add(slotNumber, v);
            }
            return (T)v;
#elif NETSTANDARD || NET4_6
            var thread = slot as ThreadLocal<object>;
            if (thread == null)
                throw new InvalidOperationException($"Expected ThreadLocal object. Received {slot?.GetType()}.");
            if (!thread.IsValueCreated)
            {
                if (!create)
                    return null;

                thread.Value = new T();
            }
            return (T)thread.Value;
#else
            LocalDataStoreSlot localDataStoreSlot = (LocalDataStoreSlot)slot;
            object v = Thread.GetData(localDataStoreSlot);
            if (v == null)
            {
                if (!create)
                    return null;

                v = new T();
                Thread.SetData(localDataStoreSlot, v);
            }

            return (T)v;
#endif
        }

#if TLS_WORKAROUND
        private static IDictionary<int, object> GetThreadDataDictionary(Thread thread)
        {
            for (var trd = threadData; trd != null; trd = trd.Next)
            {
                var t = trd.ThreadReference.Target as Thread;
                if (t == thread)
                {
                    return trd.Data;
                }
            }

            var data = new Dictionary<int, object>();
            var wtr = new WeakThreadReferenceWithData();
            wtr.ThreadReference = new WeakReference(thread);
            wtr.Data = data;

            WeakThreadReferenceWithData oldThreadData;

            do
            {
                oldThreadData = threadData;
                wtr.Next = oldThreadData;
            }
            while (Interlocked.CompareExchange(ref threadData, wtr, oldThreadData) != oldThreadData);

            return data;
        }

        internal class WeakThreadReferenceWithData
        {
            internal WeakReference ThreadReference { get; set; }

            internal IDictionary<int, object> Data { get; set; }

            internal WeakThreadReferenceWithData Next { get; set; }
        }
#endif
    }
}