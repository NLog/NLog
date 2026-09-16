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

namespace NLog.Targets
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Writes log messages to <see cref="Logs"/> in memory for programmatic retrieval.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/NLog/NLog/wiki/Memory-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/NLog/NLog/wiki/Memory-target">Documentation on NLog Wiki</seealso>
    /// <example>
    /// <p>
    /// To set up the target in the <a href="https://github.com/NLog/NLog/wiki/Configuration-file">configuration file</a>,
    /// use the following syntax:
    /// </p>
    /// <code lang="XML" source="examples/targets/Configuration File/Memory/NLog.config" />
    /// <p>
    /// To set up the log target programmatically use code like this:
    /// </p>
    /// <code lang="C#" source="examples/targets/Configuration API/Memory/Simple/Example.cs" />
    /// </example>
    [Target("Memory")]
    public sealed class MemoryTarget : TargetWithLayoutHeaderAndFooter
    {
        private readonly RingBufferList<string> _logs = new RingBufferList<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        public MemoryTarget()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message:withexception=true}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public MemoryTarget(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// Gets the list of logs gathered in the <see cref="MemoryTarget"/>.
        /// </summary>
        /// <remarks>
        /// By default enumeration will block the NLog target from writing (blocks application logging). Assign <see cref="BlockingEnumeration"/> to <see langword="false"/> to prevent blocking.
        /// </remarks>
        public IList<string> Logs => _logs;

        /// <summary>
        /// Gets or sets the max number of items to have in memory. Zero or Negative means no limit.
        /// </summary>
        /// <remarks>Default: <see langword="0"/></remarks>
        /// <docgen category='Buffering Options' order='10' />
        public int MaxLogsCount
        {
            get => _logs.MaxLogsCount;
            set => _logs.MaxLogsCount = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether enumeration of <see cref="Logs"/> blocks application logging.
        /// When <see cref="MaxLogsCount"/> is used, non-blocking enumeration can skip items as the buffer wraps.
        /// </summary>
        /// <remarks>Default: <see langword="true"/>. Blocking enumeration provides a consistent view of the logs.</remarks>
        /// <docgen category='Buffering Options' order='20' />
        public bool BlockingEnumeration
        {
            get => _logs.BlockingEnumeration;
            set => _logs.BlockingEnumeration = value;
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

            if (Header != null)
            {
                _logs.Add(RenderLogEvent(Header, LogEventInfo.CreateNullEvent()));
            }
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            if (Footer != null)
            {
                _logs.Add(RenderLogEvent(Footer, LogEventInfo.CreateNullEvent()));
            }

            base.CloseTarget();
        }

        /// <summary>
        /// Renders the logging event message and adds to <see cref="Logs"/>
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            _logs.Add(RenderLogEvent(Layout, logEvent));
        }

        private sealed class RingBufferList<T> : IList<T>
        {
            private readonly List<T> _list = new List<T>();
            private int _startIndex;

            public int MaxLogsCount { get; set; }

            public bool BlockingEnumeration { get; set; } = true;

            public T this[int index]
            {
                get
                {
                    lock (_list)
                    {
                        ValidateIndex(index);
                        return _list[GetPhysicalIndex(index)];
                    }
                }
                set
                {
                    lock (_list)
                    {
                        ValidateIndex(index);
                        _list[GetPhysicalIndex(index)] = value;
                    }
                }
            }

            public int Count
            {
                get
                {
                    lock (_list)
                    {
                        return _list.Count;
                    }
                }
            }

            bool ICollection<T>.IsReadOnly => false;

            public void Add(T item)
            {
                lock (_list)
                {
                    var maxCount = MaxLogsCount;
                    if (maxCount <= 0)
                    {
                        AddAtEnd(item);
                        return;
                    }

                    while (_list.Count > maxCount)
                    {
                        // MaxLogsCount was lowered.
                        RemoveAt(0);
                    }

                    if (_list.Count == maxCount)
                    {
                        _list[_startIndex] = item;
                        if (++_startIndex == _list.Count)
                        {
                            _startIndex = 0;
                        }
                        return;
                    }

                    AddAtEnd(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (_list)
                {
                    var count = _list.Count;
                    if ((uint)index > (uint)count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    while (MaxLogsCount > 0 && _list.Count >= MaxLogsCount)
                    {
                        // MaxLogsCount was lowered.
                        RemoveAt(0);
                        count--;
                        if (index > count)
                            index = count;
                    }

                    if (index == count)
                    {
                        Add(item);
                        return;
                    }

                    var physicalIndex = GetPhysicalIndex(index);
                    _list.Insert(physicalIndex, item);
                    if (physicalIndex < _startIndex)
                        _startIndex++;
                }
            }

            public void RemoveAt(int index)
            {
                lock (_list)
                {
                    ValidateIndex(index);

                    var physicalIndex = GetPhysicalIndex(index);
                    _list.RemoveAt(physicalIndex);
                    if (physicalIndex < _startIndex)
                    {
                        _startIndex--;
                    }

                    if (_startIndex == _list.Count)
                    {
                        _startIndex = 0;
                    }
                }
            }

            void ICollection<T>.Clear()
            {
                lock (_list)
                {
                    _list.Clear();
                    _startIndex = 0;
                }
            }

            bool ICollection<T>.Contains(T item)
            {
                lock (_list)
                {
                    return _list.Contains(item);
                }
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                lock (_list)
                {
                    if (array is null)
                        throw new ArgumentNullException(nameof(array));

                    if (arrayIndex < 0)
                        throw new ArgumentOutOfRangeException(nameof(arrayIndex));

                    if (array.Length - arrayIndex < _list.Count)
                        throw new ArgumentException("The destination array is too small.");

                    for (var i = 0; i < _list.Count; i++)
                    {
                        array[arrayIndex + i] = _list[GetPhysicalIndex(i)];
                    }
                }
            }

            public int IndexOf(T item)
            {
                lock (_list)
                {
                    var comparer = EqualityComparer<T>.Default;
                    for (var i = 0; i < _list.Count; i++)
                    {
                        if (comparer.Equals(_list[GetPhysicalIndex(i)], item))
                        {
                            return i;
                        }
                    }

                    return -1;
                }
            }

            bool ICollection<T>.Remove(T item)
            {
                lock (_list)
                {
                    var index = IndexOf(item);
                    if (index < 0)
                        return false;

                    RemoveAt(index);
                    return true;
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                int startIndex;
                int count;
                lock (_list)
                {
                    startIndex = _startIndex;
                    count = _list.Count;
                    if (count == 0)
                        return System.Linq.Enumerable.Empty<T>().GetEnumerator();
                }

                return Enumerate(startIndex, count).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            private IEnumerable<T> Enumerate(int startIndex, int count)
            {
                var cursor = startIndex;

                if (BlockingEnumeration)
                {
                    lock (_list)
                    {
                        if (_list.Count == 0)
                            yield break;

                        cursor = _startIndex;
                        do
                        {
                            yield return _list[cursor];

                            if (++cursor == _list.Count)
                                cursor = 0;
                        }
                        while (cursor != _startIndex);
                    }
                    yield break;
                }

                // Enumerate until meeting original startIndex, or reach original count to avoid infinite loop.
                var remaining = count;
                do
                {
                    T item;

                    lock (_list)
                    {
                        if (_list.Count == 0 || cursor >= _list.Count || startIndex >= _list.Count)
                            yield break;

                        item = _list[cursor];
                        if (++cursor == _list.Count)
                            cursor = 0;
                    }

                    yield return item;
                }
                while (--remaining > 0 && cursor != startIndex);
            }

            private void AddAtEnd(T item)
            {
                if (_startIndex == 0)
                {
                    _list.Add(item);
                    return;
                }

                _list.Insert(_startIndex, item);
                _startIndex++;
            }

            private int GetPhysicalIndex(int logicalIndex)
            {
                var index = _startIndex + logicalIndex;
                if (index >= _list.Count)
                {
                    index -= _list.Count;
                }
                return index;
            }

            private void ValidateIndex(int index)
            {
                if ((uint)index >= (uint)_list.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
    }
}
