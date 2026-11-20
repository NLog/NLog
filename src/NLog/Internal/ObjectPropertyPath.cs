//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

namespace NLog.Internal
{
    internal struct ObjectPropertyPath
    {
        private Layouts.Layout? _rawValueCacheKey;

        public string[]? PathNames { get; private set; }

        /// <summary>
        /// Object Path to check
        /// </summary>
        public string? Value
        {
            get => PathNames?.Length > 0 ? string.Join(".", PathNames) : null;
            set
            {
                PathNames = StringHelpers.IsNullOrWhiteSpace(value) ? null : value?.SplitAndTrimTokens('.');
                _rawValueCacheKey = PathNames != null ? new ObjectPathRawValueLayout() : null;
            }
        }

        public bool TryGetCachedRawValue(LogEventInfo logEvent, out object? rawValue, out bool found)
        {
            if (_rawValueCacheKey != null && logEvent.TryGetCachedLayoutValue(_rawValueCacheKey, out var cachedValue))
            {
                if (ReferenceEquals(cachedValue, _rawValueCacheKey))
                {
                    rawValue = null;
                    found = false;
                }
                else
                {
                    rawValue = cachedValue;
                    found = true;
                }
                return true;
            }

            rawValue = null;
            found = false;
            return false;
        }

        public bool TryCacheRawValue(LogEventInfo logEvent, object? rawValue, bool found)
        {
            if (_rawValueCacheKey is null)
                return false;

            if (!found)
            {
                logEvent.AddCachedLayoutValue(_rawValueCacheKey, _rawValueCacheKey);
                return true;
            }

            if (rawValue is null || Layouts.SimpleLayout.IsRawValueImmutable(rawValue))
            {
                logEvent.AddCachedLayoutValue(_rawValueCacheKey, rawValue);
                return true;
            }

            return false;
        }

        private sealed class ObjectPathRawValueLayout : Layouts.Layout
        {
            protected override string GetFormattedMessage(LogEventInfo logEvent) => string.Empty;
        }
    }
}
