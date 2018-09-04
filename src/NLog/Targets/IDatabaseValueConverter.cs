// 
// Copyright (c) 2004-2018 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
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

#if !SILVERLIGHT && !__IOS__ && !__ANDROID__

using System.Data;
using JetBrains.Annotations;

namespace NLog.Targets
{
    /// <summary>
    /// Convert values for the database target
    /// </summary>
    public interface IDatabaseValueConverter
    {
        /// <summary>
        /// Convert layout value to parameter value
        /// </summary>
        /// <param name="value">Current value after rendering.</param>
        /// <param name="dbType">Configured DbType, or DbType after setting the SqlDbType/OracleDyType property</param>
        /// <param name="parameterInfo">The configured parameterInfo.</param>
        /// <returns>Converted object that suits with <paramref name="dbType"/>.</returns>
        object ConvertFromString(string value, DbType dbType, [NotNull] DatabaseParameterInfo parameterInfo);

        /// <summary>
        /// Convert rawvalue to parameter value
        /// </summary>
        /// <param name="rawValue">Current rawvalue after rendering raw (non-string).</param>
        /// <param name="dbType">Configured DbType, or DbType after setting the SqlDbType/OracleDyType property</param>
        /// <param name="parameterInfo">The configured parameterInfo.</param>
        /// <returns>Converted object that suits with <paramref name="dbType"/>.</returns>
        object ConvertFromObject(object rawValue, DbType dbType, [NotNull] DatabaseParameterInfo parameterInfo);
    }
}

#endif