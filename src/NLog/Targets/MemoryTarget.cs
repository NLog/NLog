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
    using System.Collections.Generic;

    /// <summary>
    /// Writes log messages to <see cref="Logs"/> in memory for programmatic retrieval.
    /// </summary>
    /// <remarks>
    /// <a href="https://github.com/nlog/nlog/wiki/Memory-target">See NLog Wiki</a>
    /// </remarks>
    /// <seealso href="https://github.com/nlog/nlog/wiki/Memory-target">Documentation on NLog Wiki</seealso>
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
        private readonly ThreadSafeList<string> _logs = new ThreadSafeList<string>();

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
        /// Be careful when enumerating, as NLog target is blocked from writing during enumeration (blocks application logging)
        /// </remarks>
        public IList<string> Logs => _logs;

#if !NET35
        /// <summary>
        /// Gets <see cref="Logs"/> as <see cref="System.Collections.Specialized.INotifyCollectionChanged"/>
        /// to support GUI data binding.
        /// </summary>
        /// <remarks>
        /// cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"
        /// When binding to a UI framework such as WPF or MAUI, the consumer is responsible for
        /// marshaling to the UI thread. For WPF, use <c>BindingOperations.EnableCollectionSynchronization</c>.
        /// For MAUI/other, dispatch via <c>SynchronizationContext</c> or <c>MainThread.BeginInvokeOnMainThread</c>.
        /// </remarks>
        public System.Collections.Specialized.INotifyCollectionChanged ObservableLogs => _logs;
#endif

        /// <summary>
        /// Gets or sets the max number of items to have in memory. Zero or Negative means no limit.
        /// </summary>
        /// <remarks>Default: <c>0</c></remarks>
        /// <docgen category='Buffering Options' order='10' />
        public int MaxLogsCount { get; set; }

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
            _logs.Add(RenderLogEvent(Layout, logEvent), MaxLogsCount);
        }

        private sealed class ThreadSafeList<T> : IList<T>
#if !NET35
         , System.Collections.Specialized.INotifyCollectionChanged
#endif
        {
            private readonly List<T> _list = new List<T>();

#if !NET35
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler? CollectionChanged;   
        private void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
#endif

            public T this[int index]
            {
                get
                {
                    lock (_list)
                    {
                        return _list[index];
                    }
                }
                set
                {
                    T oldItem;
                    lock (_list)
                    {
                        oldItem = _list[index];
                        _list[index] = value;
                    }
#if !NET35
            if (CollectionChanged != null)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Replace, value, oldItem, index));
#endif
                }
            }

            public int Count => _list.Count;
            bool ICollection<T>.IsReadOnly => ((ICollection<T>)_list).IsReadOnly;

            public void Add(T item, int maxListCount)
            {
#if !NET35
                bool itemsRemoved = false;
#endif
                int newIndex;
                lock (_list)
                {
                    if (maxListCount > 0)
                    {
                        while (_list.Count >= maxListCount)
                        {
                            _list.RemoveAt(0);
#if !NET35
                            itemsRemoved = true;
#endif
                        }
                    }
                    _list.Add(item);
                    newIndex = _list.Count - 1;
                }
#if !NET35
             if (CollectionChanged != null)
             {
                 if (itemsRemoved)
                 {
                     OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                         System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
                 }
                 else
                 {
                     OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                         System.Collections.Specialized.NotifyCollectionChangedAction.Add, item, newIndex));
                 }
              }
#endif
            }

            public void Add(T item)
            {
                int newIndex;
                lock (_list)
                {
                    _list.Add(item);
                    newIndex = _list.Count - 1;
                }
#if !NET35
            if (CollectionChanged != null)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                   System.Collections.Specialized.NotifyCollectionChangedAction.Add, item, newIndex));
#endif
            }

            void ICollection<T>.Clear()
            {
                lock (_list) _list.Clear();
#if !NET35
            if (CollectionChanged != null)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
#endif
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
                    _list.CopyTo(array, arrayIndex);
                }
            }

            public int IndexOf(T item)
            {
                lock (_list)
                {
                    return _list.IndexOf(item);
                }
            }

            public void Insert(int index, T item)
            {
                lock (_list)
                {
                    _list.Insert(index, item);
                }
#if !NET35
            if (CollectionChanged != null)
                 OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                       System.Collections.Specialized.NotifyCollectionChangedAction.Add, item, index));
#endif
            }

            bool ICollection<T>.Remove(T item)
            {
                int index;
                lock (_list)
                {
                    index = _list.IndexOf(item);
                    if (index < 0) return false;
                    _list.RemoveAt(index);
                }
#if !NET35
            if (CollectionChanged != null)
                 OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Remove, item, index));
#endif
                return true;
            }

            public void RemoveAt(int index)
            {
                T item;
                lock (_list)
                {
                    item = _list[index];
                    _list.RemoveAt(index);
                }
#if !NET35
            if (CollectionChanged != null)
                OnCollectionChanged(new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                    System.Collections.Specialized.NotifyCollectionChangedAction.Remove, item, index));
#endif
            }

            public IEnumerator<T> GetEnumerator()
            {
                lock (_list)
                {
                    foreach (var item in _list)
                        yield return item;
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                lock (_list)
                {
                    foreach (var item in _list)
                        yield return item;
                }
            }
        }
    }
}
