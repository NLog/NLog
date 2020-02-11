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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace NLog.Internal
{
    internal static class CollectionExtensions
    {
        /// <summary>
        /// Memory optimized filtering
        /// </summary>
        /// <remarks>Passing state too avoid delegate capture and memory-allocations.</remarks>
        [NotNull]
        public static IList<TItem> Filter<TItem, TState>([NotNull] this IList<TItem> items, TState state, Func<TItem, TState, bool> filter)
        {
            var hasIgnoredLogEvents = false;
            IList<TItem> filterLogEvents = null;
            for (var i = 0; i < items.Count; ++i)
            {
                var item = items[i];
                if (filter(item, state))
                {
                    if (hasIgnoredLogEvents && filterLogEvents == null)
                    {
                        filterLogEvents = new List<TItem>();
                    }

                    filterLogEvents?.Add(item);
                }
                else
                {
                    if (!hasIgnoredLogEvents && i > 0)
                    {
                        filterLogEvents = new List<TItem>();
                        for (var j = 0; j < i; ++j)
                            filterLogEvents.Add(items[j]);
                    }
                    hasIgnoredLogEvents = true;
                }
            }
            return filterLogEvents ?? (hasIgnoredLogEvents ? ArrayHelper.Empty<TItem>() : items);
        }
    }
}
