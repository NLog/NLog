// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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
using System.ComponentModel;
using System.Threading;
using NLog.Common;

namespace NLog.Internal.PoolFactory
{
    internal sealed class LogEventPoolFactory : ILogEventObjectFactory
    {
        readonly ILogEventObjectFactory _newFactory = LogEventObjectFactory.Instance;

        readonly List<IPoolObjectFactory> _objectPools = new List<IPoolObjectFactory>();

        readonly PoolObjectFactoryT<LogEventInfo> _logEventPool;
        readonly PoolObjectFactoryT<SingleCallContinuation> _singleCallPool;
        readonly PoolObjectFactoryT<ExceptionHandlerContinuation> _exceptionHandlerPool;
        readonly PoolObjectFactoryT<CompleteWhenAllContinuation> _continueWhenAllPool;

        readonly PoolObjectFactoryT<StringBuilderTarget> _smallBuilderPool;
        readonly PoolObjectFactoryT<StringBuilderTarget> _bigBuilderPool;
        readonly PoolObjectFactoryT<MemoryStreamTarget> _smallStreamPool;
        readonly PoolObjectFactoryT<MemoryStreamTarget> _bigStreamPool;

        readonly PoolObjectFactoryT<AsyncLogEventArray> _smallLogEventArray;
        readonly PoolObjectFactoryT<AsyncLogEventArray> _bigLogEventArray;
        readonly PoolObjectFactoryT<AsyncLogEventArray> _hugeLogEventArray;

        public List<PoolStats> GetPoolsStats()
        {
            List<PoolStats> poolStats = new List<PoolStats>(_objectPools.Count);
            foreach (var objectPool in _objectPools)
            {
                if (objectPool.HasBeenUsed)
                    poolStats.Add(objectPool.GetPoolStats(true));
            }
            return poolStats;
        }

        public void GetPoolsStats(System.Text.StringBuilder builder)
        {
            builder.AppendFormat("[{0}]", OwnerName);
            builder.AppendLine();
            foreach (PoolStats stats in GetPoolsStats())
            {
                builder.AppendLine(stats.ToString());
            }
        }

        public string OwnerName { get; private set; }
        public PoolSetup PoolSetup { get; private set; }

        public LogEventPoolFactory(string ownerName, PoolSetup poolSetup, bool ownerLogger, int ownerQueueLength)
        {
            OwnerName = ownerName;
            PoolSetup = poolSetup;

            int logEventInitialSize = ownerQueueLength > 0 ? Math.Min(ownerQueueLength * 2 + 1000, 100000) : 1000;
            int logEventMaxSize = Math.Max(logEventInitialSize, 10000);
            if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
            {
                logEventInitialSize = Math.Max(logEventInitialSize, 10000);
                logEventMaxSize = Math.Max(logEventInitialSize, 100000);
            }
            if ((PoolSetup & PoolSetup.FixedSize) == PoolSetup.FixedSize)
                logEventInitialSize = logEventMaxSize;
            else if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
                logEventMaxSize = 150000;
            else if (ownerQueueLength <= 0)
                logEventMaxSize = 100000;

            bool ownerAsyncTarget = ownerQueueLength > 1;

            RegisterPool(ref _logEventPool, "LogEvent", ownerLogger ? logEventInitialSize : 0, logEventMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _singleCallPool, "SingleCont", (ownerLogger || ownerAsyncTarget) ? logEventInitialSize : 0, logEventMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _exceptionHandlerPool, "ExceptionCont", (ownerLogger || ownerAsyncTarget) ? logEventInitialSize : 0, logEventMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _continueWhenAllPool, "WhenAllCont", (ownerLogger || ownerAsyncTarget) ? logEventInitialSize : 0, logEventMaxSize, ownerAsyncTarget, poolSetup);

            int streamInitialSize = 10;
            int streamMaxSize = 100;
            if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
            {
                streamInitialSize = 100;
                streamMaxSize = 1000;
            }
            if ((PoolSetup & PoolSetup.FixedSize) == PoolSetup.FixedSize)
                streamInitialSize = streamMaxSize;
            else if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
                streamMaxSize = 1500;
            else
                streamMaxSize = 1000;
            RegisterPool(ref _smallBuilderPool, "SmallBuilder", streamInitialSize, streamMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _bigBuilderPool, "BigBuilder", streamInitialSize, streamMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _smallStreamPool, "SmallStream", streamInitialSize, streamMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _bigStreamPool, "BigStream", streamInitialSize, streamMaxSize, ownerAsyncTarget, poolSetup);

            int arrayInitialSize = 2;
            int arrayMaxSize = 4;
            if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
            {
                arrayInitialSize = 4;
                arrayMaxSize = 10;
            }
            if ((PoolSetup & PoolSetup.FixedSize) == PoolSetup.FixedSize)
                arrayInitialSize = arrayMaxSize;
            else if ((PoolSetup & PoolSetup.Large) == PoolSetup.Large)
                arrayMaxSize = 1500;
            else
                arrayMaxSize = 1000;

            RegisterPool(ref _smallLogEventArray, "SmallEventArray", ownerAsyncTarget ? arrayInitialSize : 0, arrayMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _bigLogEventArray, "BigEventArray", ownerAsyncTarget ? arrayInitialSize : 0, arrayMaxSize, ownerAsyncTarget, poolSetup);
            RegisterPool(ref _hugeLogEventArray, "HugeEventArray", ownerAsyncTarget ? arrayInitialSize : 0, arrayMaxSize, ownerAsyncTarget, poolSetup);
        }

        private void RegisterPool<T>(ref PoolObjectFactoryT<T> pool, string poolDescription, int initialSize, int maxSize, bool ownerAsyncTarget, PoolSetup poolSetup) where T : class, IPoolObject
        {
            if (poolSetup == PoolSetup.Active && !ownerAsyncTarget)
                pool = new AdaptivePoolObjectFactoryT<T>(poolDescription, initialSize, maxSize);
            else
                pool = new PoolObjectFactoryT<T>(poolDescription, initialSize, maxSize);
            _objectPools.Add(pool);
        }

        public new string ToString()
        {
            return OwnerName;
        }

        public LogEventInfo CreateLogEvent(LogLevel level, string loggerName, IFormatProvider formatProvider, [Localizable(false)] string message, object[] parameters, Exception exception)
        {
            LogEventInfo logEvent = _logEventPool.TryPop();
            if (logEvent == null)
                logEvent = new LogEventInfo(this);
            logEvent.Init(level, loggerName, formatProvider, message, parameters, exception);

            ((IPoolObject)logEvent).Owner = this;
            logEvent.Init(level, loggerName, formatProvider, message, parameters, exception);
            return logEvent;
        }

        public void ReleaseLogEvent(LogEventInfo item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();
            _logEventPool.TryClearPush(item);
        }

        const int SmallBuilderMaxSize = 1024;

        public StringBuilderTarget CreateStringBuilder(int capacity = 0)
        {
            if (capacity < SmallBuilderMaxSize)
            {
                StringBuilderTarget builder = _smallBuilderPool.TryPop();
                if (builder != null)
                {
                    ((IPoolObject)builder).Owner = this;
                    return builder;
                }
            }

            StringBuilderTarget bigBuilder = _bigBuilderPool.TryPop();
            if (bigBuilder != null)
            {
                ((IPoolObject)bigBuilder).Owner = this;
                return bigBuilder;
            }

            StringBuilderTarget newBuilder = _newFactory.CreateStringBuilder(capacity < 256 ? 256 : capacity);
            ((IPoolObject)newBuilder).Owner = this;
            return newBuilder;
        }

        public void ReleaseStringBuilder(StringBuilderTarget item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            if (item.Result.Capacity < SmallBuilderMaxSize)
                _smallBuilderPool.TryClearPush(item);
            else
                _bigBuilderPool.TryClearPush(item);
        }

        const int SmallStreamMaxSize = 2048;

        public MemoryStreamTarget CreateMemoryStream(int capacity = 0)
        {
            if (capacity < SmallStreamMaxSize)
            {
                MemoryStreamTarget builder = _smallStreamPool.TryPop();
                if (builder != null)
                {
                    ((IPoolObject)builder).Owner = this;
                    return builder;
                }
            }

            MemoryStreamTarget bigStream = _bigStreamPool.TryPop();
            if (bigStream != null)
            {
                ((IPoolObject)bigStream).Owner = this;
                return bigStream;
            }

            MemoryStreamTarget newStream = _newFactory.CreateMemoryStream(capacity < 1024 ? 1024 : capacity);
            ((IPoolObject)newStream).Owner = this;
            return newStream;
        }

        public void ReleaseMemoryStream(MemoryStreamTarget item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            if (item.Result.Capacity < SmallStreamMaxSize)
                _smallStreamPool.TryClearPush(item);
            else
                _bigStreamPool.TryClearPush(item);
        }

        const int SmallArrayMaxSize = 1000;
        const int BigArrayMaxSize = 10000;
        const int HugeArrayMaxSize = 100000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public AsyncLogEventArray CreateAsyncLogEventArray(int capacity = 0)
        {
            AsyncLogEventArray item = null;
            if (capacity <= SmallArrayMaxSize)
            {
                capacity = 1000;
                item = _smallLogEventArray.TryPop();
            }
            else if (capacity <= BigArrayMaxSize)
            {
                capacity = 10000;
                item = _bigLogEventArray.TryPop();
            }
            else if (capacity <= HugeArrayMaxSize)
            {
                capacity = 100000;
                item = _hugeLogEventArray.TryPop();
            }

            if (item == null)
                item = _newFactory.CreateAsyncLogEventArray(capacity);
            ((IPoolObject)item).Owner = this;
            return item;
        }

        public void ReleaseAsyncLogEventArray(AsyncLogEventArray item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            if (item.Buffer.Length <= SmallArrayMaxSize)
                _smallLogEventArray.TryClearPush(item);
            else if (item.Buffer.Length <= BigArrayMaxSize)
                _bigLogEventArray.TryClearPush(item);
            else if (item.Buffer.Length <= HugeArrayMaxSize)
                _hugeLogEventArray.TryClearPush(item);
        }

        public SingleCallContinuation CreateSingleCallContinuation(AsyncContinuation asyncContinuation)
        {
            SingleCallContinuation singleCall = _singleCallPool.TryPop();
            if (singleCall != null)
                singleCall.Init(asyncContinuation);
            else
                singleCall = _newFactory.CreateSingleCallContinuation(asyncContinuation);

            ((IPoolObject)singleCall).Owner = this;
            return singleCall;
        }

        public void ReleaseSingleCallContinuation(SingleCallContinuation item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            _singleCallPool.TryClearPush(item);
        }

        public ExceptionHandlerContinuation CreateExceptionHandlerContinuation(int originalThreadId, bool throwExceptions)
        {
            ExceptionHandlerContinuation exceptionHandler = _exceptionHandlerPool.TryPop();
            if (exceptionHandler != null)
                exceptionHandler.Init(originalThreadId, throwExceptions);
            else
                exceptionHandler = _newFactory.CreateExceptionHandlerContinuation(originalThreadId, throwExceptions);
            ((IPoolObject)exceptionHandler).Owner = this;
            return exceptionHandler;
        }

        public void ReleaseExceptionHandlerContinuation(ExceptionHandlerContinuation item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            _exceptionHandlerPool.TryClearPush(item);
        }

        public CompleteWhenAllContinuation CreateCompleteWhenAllContinuation()
        {
            CompleteWhenAllContinuation item = _continueWhenAllPool.TryPop();
            if (item == null)
                item = _newFactory.CreateCompleteWhenAllContinuation();
            ((IPoolObject)item).Owner = this;
            return item;
        }

        public void ReleaseCompleteWhenAllContinuation(CompleteWhenAllContinuation item)
        {
            if (((IPoolObject)item).Owner != this)
                throw new InvalidOperationException();

            _continueWhenAllPool.TryClearPush(item);
        }

        internal struct PoolStats
        {
            public readonly string PoolName;
            public readonly int Count;
            public readonly int MaxCapacity;
            public readonly uint CacheHits;
            public readonly uint CacheMisses;
            public readonly uint CacheOverflows;

            public PoolStats(string poolName, int count, int maxCapacity, uint cacheHits, uint cacheMisses, uint cacheOverflows)
            {
                PoolName = poolName;
                Count = count;
                MaxCapacity = maxCapacity;
                CacheHits = cacheHits;
                CacheMisses = cacheMisses;
                CacheOverflows = cacheOverflows;
            }

            public new string ToString()
            {
                return string.Format("{0} Count={1}, MaxSize={2}, Hits={3}, Misses={4}, Overflows={5}", PoolName, Count, MaxCapacity, CacheHits, CacheMisses, CacheOverflows);
            }
        }

        internal interface IPoolObjectFactory
        {
            bool HasBeenUsed { get; }
            PoolStats GetPoolStats(bool clearStats);
        }

        internal class PoolObjectFactorStackT<T> : IPoolObjectFactory where T : class, IPoolObject
        {
            readonly Stack<T> _container;
            readonly int _maxCapacity;
            protected readonly string _poolDescription;
            protected int _cacheHits;
            protected int _cacheMisses;
            protected int _cacheOverflows;

            public PoolObjectFactorStackT(string poolDescription, int initialSize, int maxSize)
            {
                _container = new Stack<T>(initialSize);
                _maxCapacity = maxSize;
                _poolDescription = poolDescription;
            }

            public virtual T TryPop()
            {
                if (_container.Count <= 0)
                {
                    ++_cacheMisses;
                    return null;
                }

                ++_cacheHits;
                return _container.Pop();
            }

            public bool TryClearPush(T item)
            {
                item.Owner = null;
                item.Clear();   // Clear without holding locks
                return TryPush(item);
            }

            protected virtual bool TryPush(T item)
            {
                if (_container.Count < _maxCapacity)
                {
                    _container.Push(item);
                    return true;
                }
                ++_cacheOverflows;
                return false;
            }

            public bool HasBeenUsed { get { return _cacheHits != 0 || _cacheMisses != 0 || _cacheOverflows != 0; } }

            public PoolStats GetPoolStats(bool clearStats)
            {
                PoolStats poolStats = new PoolStats(_poolDescription, _container.Count, _maxCapacity, (uint)_cacheHits, (uint)_cacheMisses, (uint)_cacheOverflows);
                if (clearStats)
                {
                    ClearStats();
                }
                return poolStats;
            }

            protected virtual void ClearStats()
            {
                _cacheHits = 0;
                _cacheMisses = 0;
                _cacheOverflows = 0;
            }

            public new string ToString()
            {
                return GetPoolStats(false).ToString();
            }
        }

#if NET4_0 || NET4_5
        internal class PoolObjectFactoryT<T> : PoolObjectFactorStackT<T> where T : class, IPoolObject
        {
            SpinLock _spinLock;
       
            public PoolObjectFactoryT(string poolDescription, int initialSize, int maxSize)
                :base(poolDescription, initialSize, maxSize)
            {
                _spinLock = new SpinLock(false);
            }

            public override T TryPop()
            {
                bool lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    return base.TryPop();
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }
            }

            protected override bool TryPush(T item)
            {
                bool lockTaken = false;
                try
                {
                    return base.TryPush(item);
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }
            }

            protected override void ClearStats()
            {
                bool lockTaken = false;
                try
                {
                    _spinLock.Enter(ref lockTaken);
                    base.ClearStats();
                }
                finally
                {
                    if (lockTaken)
                        _spinLock.Exit(false);
                }
            }
        }
#else
        internal class PoolObjectFactoryT<T> : PoolObjectFactorStackT<T> where T : class, IPoolObject
        {
            private readonly object _syncRoot = new object();

            public PoolObjectFactoryT(string poolDescription, int initialSize, int maxSize)
                : base(poolDescription, initialSize, maxSize)
            {
            }

            public override T TryPop()
            {
                lock (_syncRoot)
                {
                    return base.TryPop();
                }
            }

            protected override bool TryPush(T item)
            {
                lock (_syncRoot)
                {
                    return base.TryPush(item);
                }
            }

            protected override void ClearStats()
            {
                lock (_syncRoot)
                {
                    base.ClearStats();
                }
            }
        }
#endif

        internal class AdaptivePoolObjectFactoryT<T> : PoolObjectFactoryT<T> where T : class, IPoolObject
        {
            T _primaryContainer;
            bool _normalContainer;

            public AdaptivePoolObjectFactoryT(string poolDescription, int initialSize, int maxSize)
                : base(poolDescription, initialSize, maxSize)
            {
            }

            public new bool HasBeenUsed { get { return _normalContainer ? base.HasBeenUsed : false; } }

            public new PoolStats GetPoolStats(bool clearStats)
            {
                return _normalContainer ? base.GetPoolStats(clearStats) : new PoolStats(_poolDescription, 0, 1, 0, 0, 0);
            }

            public override T TryPop()
            {
                if (!_normalContainer)
                {
                    T item = Interlocked.Exchange(ref _primaryContainer, null);
                    if (item != null)
                        return item;
                    if (Interlocked.Increment(ref _cacheMisses) == 1)
                        return null;    // First cache miss is allowed
                    _normalContainer = true;
                }
                return base.TryPop();
            }

            protected override bool TryPush(T item)
            {
                if (!_normalContainer)
                {
                    if (Interlocked.CompareExchange(ref _primaryContainer, item, null) == null)
                        return true;
                    _normalContainer = true;
                }
                return base.TryPush(item);
            }
        }
    }
}
