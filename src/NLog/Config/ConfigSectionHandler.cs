// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !NETSTANDARD

namespace NLog.Config
{
    using System;
    using System.Configuration;
    using System.Xml;
    using NLog.Common;
    using NLog.Internal;

    /// <summary>
    /// Represents NLog ConfigSection for loading <see cref="XmlLoggingConfiguration"/> from app.config / web.config
    /// </summary>
    /// <remarks><code>
    /// &lt;configSections&gt;
    ///   &lt;section name=&quot;NLog&quot; type=&quot;NLog.Config.ConfigSectionHandler, NLog&quot; /&gt;
    /// &lt;/configSections&gt;
    /// </code></remarks>
    public sealed class ConfigSectionHandler : ConfigurationSection
    {
        private XmlLoggingConfiguration _config;

        /// <summary>
        /// Overriding base implementation to just store <see cref="XmlReader"/>
        /// of the relevant app.config section.
        /// </summary>
        /// <param name="reader">The XmlReader that reads from the configuration file.</param>
        /// <param name="serializeCollectionKey">true to serialize only the collection key properties; otherwise, false.</param>
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            try
            {
                string configFileName = LogFactory.DefaultAppEnvironment.AppDomainConfigurationFile;
                _config = new XmlLoggingConfiguration(reader, configFileName, LogManager.LogFactory);
            }
            catch (Exception exception)
            {
                InternalLogger.Error(exception, "Failed loading XML configuration from NLog ConfigSection in app.config");

                if (exception.MustBeRethrown())
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Override base implementation to return a <see cref="LoggingConfiguration"/> object
        /// for <see cref="System.Configuration.ConfigurationManager.GetSection(string)"/>
        /// instead of the <see cref="ConfigSectionHandler"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="LoggingConfiguration"/> instance, that has been deserialized from app.config.
        /// </returns>
        protected override object GetRuntimeObject()
        {
            return _config;
        }
    }
}
#endif