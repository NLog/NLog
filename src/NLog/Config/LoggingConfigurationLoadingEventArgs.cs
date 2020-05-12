﻿// 
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

namespace NLog.Config
{
    using System;

    /// <summary>
    /// Arguments for <see cref="LogFactory.ConfigurationLoading"/> events.
    /// </summary>
    public class LoggingConfigurationLoadingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingConfigurationLoadingEventArgs" /> class.
        /// </summary>
        /// <param name="activatingConfiguration">The new configuration.</param>
        /// <param name="deactivatingConfiguration">The old configuration.</param>
        public LoggingConfigurationLoadingEventArgs(LoggingConfiguration activatingConfiguration, LoggingConfiguration deactivatingConfiguration)
        {
            ActivatingConfiguration = activatingConfiguration;
            DeactivatingConfiguration = deactivatingConfiguration;
        }

        /// <summary>
        /// Gets the old configuration.
        /// </summary>
        public LoggingConfiguration DeactivatingConfiguration { get; private set; }

        /// <summary>
        /// Gets the new configuration.
        /// </summary>
        public LoggingConfiguration ActivatingConfiguration { get; private set; }

        /// <summary>
        /// The <see cref="ActivatingConfiguration"/> is being loaded for the first time
        /// </summary>
        public bool IsNewConfiguration { get; internal set; }
    }
}
