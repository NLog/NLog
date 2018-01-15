// 
// Copyright (c) 2004-2017 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__ && !NETSTANDARD

namespace NLog.Targets
{

    using System.Data.Common;
    using NLog.Common;
    using NLog.Layouts;
    using NLog.Internal;

    /// <summary>
    /// An NLog target that writes logging messages using an ADO.NET provider that is configurable via an Entity Framework connection string.
    /// </summary>
    [Target("EntityFramework")]
    public class EntityFrameworkTarget : DatabaseTarget
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkTarget" /> class.
        /// </summary>
        public EntityFrameworkTarget()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkTarget" /> class.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        public EntityFrameworkTarget(string name) : this() => Name = name;

        /// <summary>
        /// Initializes the target. Can be used by inheriting classes
        /// to initialize logging.
        /// </summary>
        protected override void InitializeTarget()
        {
            base.InitializeTarget();

#pragma warning disable CS0618 // Type or member is obsolete
            if (UseTransactions.HasValue)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                InternalLogger.Warn("UseTransactions is obsolete and will not be used - will be removed in NLog 6");
            }

            if (string.IsNullOrEmpty(ConnectionStringName))
            {
                throw new NLogConfigurationException("ConnectionStringName is required parameter.");
            }
            // read connection string and provider factory from entity framework connection string
            var cs = ConnectionStringsSettings[ConnectionStringName];
            if (cs == null)
            {
                throw new NLogConfigurationException("Connection string '" + ConnectionStringName + "' is not declared in <connectionStrings /> section.");
            }

            var dbConnectionStringBuilder = new DbConnectionStringBuilder { ConnectionString = cs.ConnectionString };

            //CurrentValues Dictionary in DbConnectionStringBuilder is case insensitive (StringComparer.OrdinalIgnoreCase)
            string provider = (string)dbConnectionStringBuilder["provider"];

            if (StringHelpers.IsNullOrWhiteSpace(provider))
            {
                throw new NLogConfigurationException("Provider not found");
            }

            ProviderFactory = DbProviderFactories.GetFactory(provider);

            string connectionString = (string)dbConnectionStringBuilder["provider connection string"];

            if (StringHelpers.IsNullOrWhiteSpace(connectionString))
            {
                throw new NLogConfigurationException("Connection string not found or empty.");
            }

            ConnectionString = SimpleLayout.Escape(connectionString);
        }
    }
}
#endif
