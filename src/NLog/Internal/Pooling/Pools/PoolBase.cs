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
using System.Text;
using System.Threading;

using NLog.Config;

namespace NLog.Internal.Pooling.Pools
{
    /// <summary>
    /// Base class for object pools containing common functionality.
    /// </summary>
    internal abstract class PoolBase : ISupportsInitialize
    {
        private int poolSize;
        private bool preFill;
        private bool initialized;
        // counter for number of items in the pool

        long givenOut;
        long gottenBack;
        private bool autoIncrease;

        private bool enabled;
        private readonly char[] poolName;
        private readonly char[] newLine = Environment.NewLine.ToCharArray();

        private readonly char[] statsBuffer = new char[50];

        /// <summary>
        /// Initializes an object poolpool
        /// </summary>
        /// <param name="poolSize">The size of the pool, in number of objects.</param>
        /// <param name="preFill">Whether or not to automatically prefill the pool with objects.</param>
        /// <param name="poolName">The name of the pool for logging purposes.</param>
        /// <param name="enabled">Whether or not the pool is enabled.</param>
        protected PoolBase(int poolSize, bool preFill = false, string poolName = null, bool enabled = false)
        {
            this.poolSize = poolSize;
            this.preFill = preFill;

            this.enabled = enabled;
            this.poolName = string.IsNullOrEmpty(poolName) ? this.GetType().FullName.ToCharArray() : poolName.ToCharArray();
        }

        /// <summary>
        /// Resets pool back to newly created
        /// </summary>
        /// <param name="newSize">The new size of the pool</param>
        protected void Reset(int newSize)
        {
            this.poolSize = newSize;
            this.givenOut = 0;
            this.gottenBack = 0;
        }

        /// <summary>
        /// Gets the number of pooled objects currently in use.
        /// This number can be high, if some code fails and fail to put objects back in the pool
        /// </summary>
        public long InUse
        {
            get
            {
                unchecked
                {
                    return this.givenOut - this.gottenBack;
                }
            }
        }

        /// <summary>
        /// Gets the number of objects currently in the pool.
        /// This does not take into account the number of objects in use.
        /// So real number of pooled objects can be higher.
        /// </summary>
        public abstract long ObjectsInPool
        {
            get;
        }

        /// <summary>
        /// Gets the number of thrown away objects - a high number could indicate that your pool size is too small.
        /// </summary>
        public abstract long ThrownAwayObjects
        {
            get;
        }

        /// <summary>
        /// Gets the max size of the pool. 
        /// This does not take into account if pool is set to auto increase, 
        /// then the number of items in the pool can be higher than this number.
        /// </summary>
        public int PoolSize
        {
            get
            {
                return this.poolSize;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the pool is enabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this pool was prefilled.
        /// </summary>
        public bool PreFill
        {
            get
            {
                return this.preFill;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the pool has been initialized (started)
        /// </summary>
        public bool Initialized
        {
            get
            {
                return this.initialized;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the pool supports auto increase, 
        /// when items are being put back into the pool or if excess items should be discarded.
        /// </summary>
        public bool AutoIncrease
        {
            get
            {
                return this.autoIncrease;
            }
        }

        /// <summary>
        /// Gets the name of the pool as a char array for logging purposes
        /// </summary>
        public char[] Name
        {
            get
            {
                return this.poolName;
            }
        }

        /// <summary>
        /// Increments the counter for objects gotten back.
        /// </summary>
        internal void GottenBack()
        {
            Interlocked.Increment(ref this.gottenBack);
        }

        /// <summary>
        /// Increments the counter for objects given out.
        /// </summary>
        internal void GivenOut()
        {
            Interlocked.Increment(ref this.givenOut);
        }

        /// <summary>
        /// Re-Initializes the pool with the new configuration.
        /// This will in effect discard all existing objects if the pool was already enabled
        /// or shut down the pool if the new setting says so.
        /// and reconfigure the pool with the new settings.
        /// </summary>
        /// <param name="configuration">The configuration with the new settings.</param>
        public void ReInitialize(PoolConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (!this.enabled && !configuration.Enabled)
            {
                // Nothing to do
                return;
            }
            if (configuration.Enabled)
            {
                this.PoolLimitsChanged(configuration);
                this.Restart(configuration);
            }

            if (this.enabled && !configuration.Enabled)
            {
                // shut down pool, since its no loger supposed to be enabled
                this.ShutDown();
            }
        }
        
        /// <summary>
        /// Restarts the pool with the new settings
        /// </summary>
        /// <param name="configuration">The configuration with the new settings.</param>
        private void Restart(PoolConfiguration configuration)
        {
            this.preFill = configuration.PrefillPools;
            this.autoIncrease = configuration.AutoIncreasePoolSizes;

            this.poolSize = this.GetPoolSize(configuration);
            this.enabled = configuration.Enabled;

            this.RestartPool(configuration);
        }

        /// <summary>
        /// Stops the pool
        /// </summary>
        private void ShutDown()
        {
            this.enabled = false;
            this.initialized = false;
            this.givenOut = 0;
            this.gottenBack = 0;
            this.EmptyPool();
        }

        /// <summary>
        /// Empties the pool of all items. Pool specific implementation.
        /// </summary>
        protected abstract void EmptyPool();

        /// <summary>
        /// Restarts the pool. Pool specific implementation.
        /// </summary>
        /// <param name="configuration">The configuration to use after the restart.</param>
        protected abstract void RestartPool(PoolConfiguration configuration);

        /// <summary>
        /// Initializes the pool with the given configuration.
        /// </summary>
        /// <param name="configuration">The new configration.</param>
        public void Initialize(PoolConfiguration configuration)
        {
            if (!this.Enabled)
            {
                return;
            }
            if (this.Initialized)
            {
                return;
            }

            this.InitializePool(configuration);

            this.initialized = true;
        }

        /// <summary>
        /// Initializes the pool with the new configuration. Pool specific implementation.
        /// </summary>
        /// <param name="configuration"></param>
        protected abstract void InitializePool(PoolConfiguration configuration);

        /// <summary>
        /// Has to be implemented in implementation.
        /// PoolBase will call this to figure out how big a pool should be dependent on the given configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The number of pooled objects this pool should support.</returns>
        protected virtual int GetPoolSize(PoolConfiguration configuration)
        {
            return configuration.EstimatedLogEventsPerSecond;
        }

        /// <summary>
        /// Implementations of pools can inspect the pool configuration and tweak their configuration based on the new configuration.
        /// Do not copy the configuration to a local variable.
        /// </summary>
        /// <param name="configuration">The new configuration to use.</param>
        protected virtual void PoolLimitsChanged(PoolConfiguration configuration)
        {
        }

        /// <summary>
        /// Outputs pool statistics to the given string builder for logging purposes.
        /// </summary>
        /// <param name="builder">The string builder to output statistics to.</param>
        internal void WriteStatsTo(StringBuilder builder)
        {
            builder.Append(this.poolName, 0, this.poolName.Length);
            builder.Append('|');

            int chars = WriteNumber(this.poolSize, this.statsBuffer, 0);
            builder.Append(this.statsBuffer, 0, chars);
            builder.Append('|');

            chars = WriteNumber(this.ObjectsInPool, this.statsBuffer, 0);
            builder.Append(this.statsBuffer, 0, chars);
            builder.Append('|');

            chars = WriteNumber(this.InUse, this.statsBuffer, 0);
            builder.Append(this.statsBuffer, 0, chars);
            builder.Append('|');

            chars = WriteNumber(this.givenOut, this.statsBuffer, 0);
            builder.Append(this.statsBuffer, 0, chars);
            builder.Append('|');

            chars = WriteNumber(this.ThrownAwayObjects, this.statsBuffer, 0);
            builder.Append(this.statsBuffer, 0, chars);
            builder.Append('|');

            builder.Append(this.newLine, 0, this.newLine.Length);
        }

        /// <summary>
        /// Resets all stats
        /// </summary>
        internal virtual void ResetStats()
        {
            this.givenOut = 0;
            this.gottenBack = 0;
        }

        /// <summary>
        /// Writes the given number as a char to the given char buffer at the requested index.
        /// </summary>
        /// <param name="value">The number to write</param>
        /// <param name="buffer">The buffer to write it to.</param>
        /// <param name="index">The index to start writint at.</param>
        /// <returns></returns>
        private static int WriteNumber(long value, char[] buffer, int index)
        {
            if (value == 0)
            {
                buffer[index] = '0';
                return 1;
            }

            // find out how many digits
            int len = 1;
            for (long rem = value / 10; rem > 0; rem /= 10)
            {
                len++;
            }

            // Move backwards in char array and write
            for (int i = len - 1; i >= 0; i--)
            {
                buffer[index + i] = (char)('0' + (value % 10));
                value /= 10;
            }
            return len;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void Initialize(LoggingConfiguration configuration)
        {
            this.Initialize(configuration.PoolConfiguration);
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            this.ShutDown();
        }
    }
}