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

namespace NLog
{
    using System;
    using NLog.Config;
    using NLog.Internal;

    /// <summary>
    /// Extension methods to setup LogFactory options
    /// </summary>
    public static class SetupBuilderExtensions
    {
        /// <summary>
        /// Configures loading of NLog extensions for Targets and LayoutRenderers
        /// </summary>
        public static ISetupBuilder SetupExtensions(this ISetupBuilder setupBuilder, Action<ISetupExtensionsBuilder> extensionsBuilder)
        {
            extensionsBuilder(new SetupExtensionsBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        /// <summary>
        /// Configures the output of NLog <see cref="Common.InternalLogger"/> for diagnostics / troubleshooting
        /// </summary>
        public static ISetupBuilder SetupInternalLogger(this ISetupBuilder setupBuilder, Action<ISetupInternalLoggerBuilder> internalLoggerBuilder)
        {
            internalLoggerBuilder(new SetupInternalLoggerBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }

        /// <summary>
        /// Configures serialization and transformation of LogEvents
        /// </summary>
        public static ISetupBuilder SetupSerialization(this ISetupBuilder setupBuilder, Action<ISetupSerializationBuilder> serializationBuilder)
        {
            serializationBuilder(new SetupSerializationBuilder(setupBuilder.LogFactory));
            return setupBuilder;
        }
    }
}
