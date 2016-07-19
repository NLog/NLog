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

using System.ComponentModel;
using NLog.Config;

namespace NLog.Internal.Pooling
{
    /// <summary>
    /// Configuration of the object pooling that can be enabled to prevent NLog
    /// from allocating objects that just has to be collected again.
    /// Enabling object pooling, will cause more memory to be used by your application, 
    /// but will cut down on the number of GC0-GC2 collections that is caused by NLog logging.
    /// </summary>
    /// <docgen category='Advanced' order='10' />
    [NLogConfigurationItem]
    [Pooling("NLogPooling")]
    public sealed class PoolConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the pool configuration.
        /// </summary>
        public PoolConfiguration() : this(null)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the pool configuration with the given logging configuration
        /// </summary>
        /// <param name="loggingConfiguration">the logging configuration to use, allowed to be null.</param>
        internal PoolConfiguration(LoggingConfiguration loggingConfiguration)
        {
            this.LoggingConfiguration = loggingConfiguration;
            this.Enabled = false;
            this.EstimatedLogEventsPerSecond = 100;
            this.EstimatedMaxMessageSize = 2048;
            this.AutoIncreasePoolSizes = false;
            this.OutputPoolStatisticsInLogFiles = false;
            this.ResetPoolStatisticsAfterReporting = true;
            this.OutputPoolStatisticsInterval = 600;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not pooling is enabled.
        /// </summary>
        [DefaultValue(false)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a number indicating how many log events NLog has to log every second on average.
        /// If you have high peak periods and need those periods to not do unnessary Garbage collection, set this
        /// number to the highest number of log events per second
        /// </summary>
        [DefaultValue(100)]
        public int EstimatedLogEventsPerSecond { get; set; }

        /// <summary>
        /// Gets or sets a number indicating the estimated max message size that the application will log.
        /// This includes any exception's string representation. This number in in char's, so
        /// if you only log exceptions and a message with them, then try to estimate the size of the exception and stack trace printed, 
        /// plus the message you log with it and add a generous number to it.
        /// This number controls how big the char buffers NLog allocates - if this number is too small, then NLog has to do more 
        /// iterations when writing the LogEvent, and possibly if its way to small, then it will have to allocate more char buffers.
        /// </summary>
        [DefaultValue(2048)]
        public int EstimatedMaxMessageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not you want NLog to output statistics
        /// for the pool once in a while when your application is running.
        /// This log will be written to all targets that logs INFO or higher.
        /// This is mostly useful when you are unsure on how big your pool sizes should be and want NLog
        /// to give you numbers that can help you tweak the pool sizes.
        /// </summary>
        /// <docgen category='Advanced' order='10' />
        [DefaultValue(false)]
        public bool OutputPoolStatisticsInLogFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds between reporting pool statistics
        /// Default 10 minutes (600 seconds)
        /// </summary>
        /// <docgen category='Advanced' order='20' />
        [DefaultValue(600)]
        public int OutputPoolStatisticsInterval { get; set; }


        /// <summary>
        /// Gets or sets the name of the logger to use for reporting statistics.
        /// If this is not set, the internal logger of NLog is used to report statistics.
        /// Set this to the name of a logger if you want the NLog pool statistics in your own log files.
        /// </summary>
        /// <docgen category='Advanced' order='30' />
        [DefaultValue(null)]
        public string PoolStatisticsLoggerName { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether or not the pool statistics should be reset after the statistics has been output.
        /// Enabling this will make it possible to see spikes of objects thrown away, instead of just an ever increasing number if any objects
        /// are thrown away.
        /// </summary>
        /// <docgen category='Advanced' order='40' />
        [DefaultValue(true)]
        public bool ResetPoolStatisticsAfterReporting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not you want to pre fill your pools with objects, 
        /// so that there will not be tiny performance hit when logging an event the first time.
        /// This should only be set on very performance sensitive applications.
        /// </summary>
        /// <docgen category='Advanced' order='50' />
        [DefaultValue(false)]
        public bool PrefillPools { get; set; }

        /// <summary>
        /// This controls whether or not you want NLOG to automatically increase a pool's size when it gets exhausted. 
        /// This helps you to automatically gain the correct number of max items in your pool, but can cause massively increased memory use
        /// in your application if you have massive spikes.
        /// Set this if you have lots of memory or you want to combine this with <see cref="OutputPoolStatisticsInLogFiles"/> to automatically
        /// find the best number for your pools.
        /// </summary>
        /// <docgen category='Advanced' order='60' />
        [DefaultValue(false)]
        public bool AutoIncreasePoolSizes { get; set; }


        /// <summary>
        /// This controls whether or not you want NLOG to use a different pool for storing its pooled objects. 
        /// When this is false, it uses a strategy with a Stack and a lock statement.
        /// If true, then its using a ringbuffer that supports many threads reading and writing at the same time.
        /// </summary>
        /// <docgen category='Advanced' order='60' />
        [DefaultValue(false)]
        public bool OptimiseForManyThreads { get; set; }

        /// <summary>
        /// Gets the current logging configuration
        /// </summary>
        internal LoggingConfiguration LoggingConfiguration { get; set; }

        /// <summary>
        /// Reconfigures the object pools, by calling PoolFactory.Reinitialize
        /// <param name="factory">The pool factory to re-initialize</param>
        /// </summary>
        internal void ReconfigurePools(PoolFactory factory)
        {
            factory.ReInitialize(this);
        }
    }
}