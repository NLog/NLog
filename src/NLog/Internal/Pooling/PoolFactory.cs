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
using System.Text;
using System.Threading;

using NLog.Common;
using NLog.Config;
using NLog.Internal.Pooling.Pools;

namespace NLog.Internal.Pooling
{
    /// <summary>
    /// Factory for initializing and getting pooled items from.
    /// If configured, this factory will log stats from the pools into either the Internal NLog Log or a configured logger.
    /// </summary>
    internal class PoolFactory : IDisposable, ISupportsInitialize
    {
        private static readonly double PoolFactor;

        private readonly List<PoolBase> pools = new List<PoolBase>();

        static PoolFactory()
        {
#if !SILVERLIGHT && !NET3_5
            PoolFactor = Environment.Is64BitProcess ? 1 : 0.25;
#else
            PoolFactor = 0.25;
#endif
        }

        private Timer statsTimer;

        /// <summary>
        /// ReInitializes the pool factory and all pools in it, with the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void ReInitialize(PoolConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.ReconfigureTimer(configuration);

            foreach (var pool in this.pools)
            {
                try
                {
                    pool.ReInitialize(configuration);
                }
                catch (Exception e)
                {
                    InternalLogger.Error("Failed to re-initialize pool:{0} because of exception:{1}", pool.Name, e);
                }
            }
        }

        /// <summary>
        /// Reconfigures the logging timer.
        /// Will either start a timer or stop it depending on the settings.
        /// </summary>
        /// <param name="configuration">the configuration.</param>
        private void ReconfigureTimer(PoolConfiguration configuration)
        {
            if (configuration.OutputPoolStatisticsInLogFiles && this.statsTimer == null && configuration.Enabled)
            {
                this.StartStatsTimer(configuration);
                return;
            }

            if (!configuration.OutputPoolStatisticsInLogFiles && this.statsTimer != null)
            {
                this.ShutdownStatsTimer();
            }

        }

        /// <summary>
        /// Starts the logging timer.
        /// </summary>
        /// <param name="configuration">the configuration.</param>
        private void StartStatsTimer(PoolConfiguration configuration)
        {
            if (this.statsTimer != null)
            {
                this.ShutdownStatsTimer();
            }

            this.statsTimer = new Timer(
                   this.ReportStats,
                   configuration,
                   configuration.OutputPoolStatisticsInterval * 1000,
                   Timeout.Infinite);
        }

        /// <summary>
        /// Shuts down the logging timer-
        /// </summary>
        private void ShutdownStatsTimer()
        {
            if (this.statsTimer != null)
            {
                this.statsTimer.Change(Timeout.Infinite, Timeout.Infinite);
                this.statsTimer.Dispose();
                this.statsTimer = null;
            }
        }

        // TODO: Add more pools, there are still stuff that gets allocated.

        /// <summary>
        /// Initializes the pool factory with the given pool configuration.
        /// Will instantiate the pools, but not start them unless configuration says so.
        /// </summary>
        /// <param name="configuration">the configuration.</param>
        public PoolFactory(PoolConfiguration configuration)
        {
            int arraySize = 90000;
            var charArrayPool = new CharArrayPool((int)(10 * arraySize * PoolFactor), (int)(arraySize * PoolFactor), configuration.PrefillPools);
            var asyncLogEventInfoListPool = new AsyncLogEventInfoListPool(5, (int)(10 * PoolFactor), configuration.PrefillPools);
            this.AddPool(charArrayPool,configuration);
            this.AddPool(asyncLogEventInfoListPool,configuration);
            this.AddPool(new ByteArrayPool((int)(10 * arraySize * PoolFactor), (int)(arraySize * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new StringBuilderPool((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new SingleCallContinuationPool((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new LogEventInfoPool((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new FileNameDictionaryPool(asyncLogEventInfoListPool, 10, 5, configuration.PrefillPools), configuration);
            this.AddPool(new AsyncContinuationListPool(10, configuration.PrefillPools), configuration);
            this.AddPool(new CombinedAsyncContinuationPool((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new GenericPool<Counter>((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new GenericPool<ContinueWhenAll>((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new ExceptionHandlerPool((int)(10 * PoolFactor), configuration.PrefillPools), configuration);
            this.AddPool(new AsyncLogEventInfoArrayPool((int)(10 * arraySize * PoolFactor), (int)(arraySize * PoolFactor)), configuration);
            this.AddPool(new MemoryStreamPool((int)(10 * PoolFactor)), configuration);
        }

        /// <summary>
        /// Timer callback method that will log the stats.
        /// </summary>
        /// <param name="o">mandatory object parameter. (Not used)</param>
        private void ReportStats(object o)
        {
            StringBuilder builder = null;
            try
            {
                PoolConfiguration configuration = (PoolConfiguration)o;

                builder = this.Get<StringBuilderPool, StringBuilder>().Get();

                builder.Append(StatsPoolNameChars, 0, StatsPoolNameChars.Length);
                builder.Append('|');
                builder.Append(StatsPoolSizeChars, 0, StatsPoolSizeChars.Length);
                builder.Append('|');
                builder.Append(StatsObjectsInPoolChars, 0, StatsObjectsInPoolChars.Length);
                builder.Append('|');
                builder.Append(StatsInUseChars, 0, StatsInUseChars.Length);
                builder.Append('|');
                builder.Append(StatsGivenOutChars, 0, StatsGivenOutChars.Length);
                builder.Append('|');
                builder.Append(StatsThrownAwayChars, 0, StatsThrownAwayChars.Length);
                builder.Append('|');
                builder.Append(StatsNewLine, 0, StatsNewLine.Length);

                foreach (var pool in this.pools)
                {
                    pool.WriteStatsTo(builder);
                }

                if (configuration.ResetPoolStatisticsAfterReporting)
                {
                    lock (this.pools)
                    {
                        foreach (var pool in this.pools)
                        {
                            pool.ResetStats();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(configuration.PoolStatisticsLoggerName))
                {
                    LogManager.GetLogger(configuration.PoolStatisticsLoggerName).Info(builder.ToString());
                }
                else
                {
                    InternalLogger.Info(builder.ToString());
                }
                this.statsTimer.Change(configuration.OutputPoolStatisticsInterval * 1000, Timeout.Infinite);
            }
            catch (Exception e)
            {
                InternalLogger.Error("Error occured while reporting pool statistics:{0}", e);
            }
            finally
            {
                if (builder != null)
                {
                    try
                    {
                        this.PutBack(builder);
                    }
                    catch (Exception e)
                    {
                        InternalLogger.Error("Error occured while putting back string builder slim into the pool:{0}", e);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a pool to the factory.
        /// </summary>
        /// <typeparam name="TPooled">The pooled item</typeparam>
        /// <param name="pool">The pool.</param>
        /// <param name="configuration">The configuration.</param>
        private void AddPool<TPooled>(PoolBaseOfT<TPooled> pool, PoolConfiguration configuration)
            where TPooled : class
        {
            PoolLookup<TPooled>.Init(pool);
            this.pools.Add(pool);
            pool.Initialize(configuration);
        }

        /// <summary>
        /// Factory Get method. Used for the special pool with special "Get" methods.
        /// For generic pools, just use Get{TPooled}
        /// </summary>
        /// <typeparam name="TPool">The type of pool.</typeparam>
        /// <typeparam name="TPooled">The pooled item.</typeparam>
        /// <returns>The pool.</returns>
        public TPool Get<TPool, TPooled>()
            where TPool : PoolBaseOfT<TPooled>
            where TPooled : class
        {
            return (TPool)PoolLookup<TPooled>.Pool;
        }

        /// <summary>
        /// Generic Factory Get method, for implementations of <see cref="IPool{TPooled}"/>
        /// </summary>
        /// <typeparam name="TPooled">The pooled item.</typeparam>
        /// <returns>The pool</returns>
        public TPooled Get<TPooled>()
            where TPooled : class, IPooledItem<TPooled>, new()
        {
            return PoolLookup<TPooled>.Pool.Get();
        }

        /// <summary>
        /// Factory put back item method.
        /// Will look up the required pool and put the item back into the pool.
        /// </summary>
        /// <typeparam name="TPooled">The pooled type.</typeparam>
        /// <param name="item">The pooled item</param>
        public void PutBack<TPooled>(TPooled item)
             where TPooled : class
        {
            PoolLookup<TPooled>.Pool.PutBack(item);
        }

        private static readonly char[] StatsPoolNameChars = "Pool Name".ToCharArray();
        private static readonly char[] StatsPoolSizeChars = "Pool Size".ToCharArray();
        private static readonly char[] StatsObjectsInPoolChars = "Objects in Pool".ToCharArray();
        private static readonly char[] StatsInUseChars = "Objects in use".ToCharArray();
        private static readonly char[] StatsGivenOutChars = "Objects given out".ToCharArray();
        private static readonly char[] StatsThrownAwayChars = "Objects thrown away".ToCharArray();
        private static readonly char[] StatsNewLine = Environment.NewLine.ToCharArray();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.ShutdownStatsTimer();
            try
            {
                foreach (var pool in this.pools)
                {
                    pool.Close();
                }
            }
            catch (Exception e)
            {
                InternalLogger.Error(e, "Error occured while shutting down pool");
            }
        }

        // Cache pools created via static instance field in generic field, which causes compiler to make an instance of each TPooled, removing the need for a dictionary lookup.
        private static class PoolLookup<TPooled>
            where TPooled : class
        {
            private static PoolBaseOfT<TPooled> pool;

            public static void Init(PoolBaseOfT<TPooled> objectPool)
            {
                pool = objectPool;
            }

            public static PoolBaseOfT<TPooled> Pool
            {
                get
                {
                    if (pool == null)
                    {
                        throw new InvalidOperationException(string.Format("No pool found for type:{0}", typeof(TPooled).FullName));
                    }
                    return pool;
                }
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void Initialize(LoggingConfiguration configuration)
        {
            this.ReInitialize(configuration.PoolConfiguration);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }
    }
}