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

using NLog.Common;
using NLog.Targets.Wrappers;

namespace NLog.Internal.Pooling.Pools
{
    internal abstract class ArrayPoolBase<TPooled> : PoolBaseOfT<TPooled[]>
    {
        protected int NumberOfTargets;

        protected int MaxQueueSize;

        /// <summary>
        /// Initializes an instance of <see cref="ArrayPoolBase{TPooled}"/> with the given max pool size and individual array size.
        /// </summary>
        /// <param name="maxPoolSize">The maximum number of elements this pool will support.</param>
        /// <param name="individualArraySize">The size of the individual arrays.</param>
        /// <param name="preFill">Whether or not to prefill the pool with  arrays.</param>
        /// <param name="poolName">the na,e of the pool, default null means name of derived type</param>
        /// <param name="enabled">Whether or not to enable pool</param>
        protected ArrayPoolBase(int maxPoolSize, int individualArraySize, bool preFill = false, string poolName = null, bool enabled = false)
            : base(maxPoolSize / individualArraySize, preFill, poolName, enabled)
        {
            if (maxPoolSize % individualArraySize != 0)
            {
                throw new ArgumentException("maxPoolSizeInBytes must be divisible by individualArraySize");
            }
            this.IndividualArraySize = individualArraySize;
        }

        public int IndividualArraySize
        {
            get;
            protected set;
        }

        protected override TPooled[] Factory()
        {
            return new TPooled[this.IndividualArraySize];
        }
        protected override int GetPoolSize(PoolConfiguration configuration)
        {
            InternalLogger.Info("Setting PoolSize for:{0} to be:{1}", this.GetType().FullName, this.NumberOfTargets);
            return this.NumberOfTargets * 4;
        }

        public override TPooled[] Get()
        {
            throw new InvalidOperationException("You have to call method that takes an array size as parameter");
        }

        public TPooled[] Get(int arraySize)
        {
            if (arraySize > this.IndividualArraySize)
            {
                // Cannot satisfy need, just return new array of requested size
                InternalLogger.Trace("Allocating array of size:{0} because its above array size of pool:{1}", arraySize.ToString(), this.IndividualArraySize.ToString());
                return new TPooled[arraySize];
            }

            return base.Get();
        }

        protected override void PoolLimitsChanged(PoolConfiguration configuration)
        {
            this.IndividualArraySize = 100;

            if (configuration.LoggingConfiguration != null)
            {
                this.NumberOfTargets = configuration.LoggingConfiguration.AllTargets.Count;
                InternalLogger.Info("Setting numberOfTargets for:{0} to be:{1}", this.GetType().FullName, this.NumberOfTargets);
                this.MaxQueueSize = 0;

                for (int x = 0; x < configuration.LoggingConfiguration.AllTargets.Count; x++)
                {
                    var target = configuration.LoggingConfiguration.AllTargets[x] as AsyncTargetWrapper;

                    if (target != null)
                    {
                        if (target.BatchSize > this.MaxQueueSize)
                        {
                            this.MaxQueueSize = target.BatchSize;
                        }
                    }
                }

                InternalLogger.Info("Setting individual array size for {0} to be:{1}", this.GetType().FullName, this.MaxQueueSize);

                // Set a good default array size to accomodate the highest batchsize of the async targets.
                this.IndividualArraySize = this.MaxQueueSize;
            }
        }

        /// <summary>
        /// Method that verifies that an item is okay to put back into the pool
        /// </summary>
        /// <param name="item">The item to verify.</param>
        /// <returns>true if the item is okay to put back; false otherwise.</returns>
        protected override bool VerifyPooledItem(TPooled[] item)
        {
            if (item.Length != this.IndividualArraySize)
            {
                return false;
            }

            return true;
        }
    }
}