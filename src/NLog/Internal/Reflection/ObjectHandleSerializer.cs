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


namespace NLog.Internal
{
#if NET35 || NET40 || NET45
    using System;
    using System.Runtime.Serialization;
    using NLog.Common;

    [Serializable]
    internal class ObjectHandleSerializer : ISerializable
    {
        [NonSerialized]
        private readonly object _wrapped;

        public ObjectHandleSerializer()
        {
        }

        public ObjectHandleSerializer(object wrapped)
        {
            _wrapped = wrapped;
        }

        protected ObjectHandleSerializer(SerializationInfo info, StreamingContext context)
        {
            Type type = null;
            try
            {
                type = (Type)info.GetValue("wrappedtype", typeof(Type));
                _wrapped = info.GetValue("wrappedvalue", type);
            }
            catch (Exception ex)
            {
                _wrapped = string.Empty;    // Type cannot be resolved in this AppDomain
                InternalLogger.Debug(ex, "ObjectHandleSerializer failed to deserialize object: {0}", type);
            }
        }

        [System.Security.Permissions.SecurityPermissionAttribute(System.Security.Permissions.SecurityAction.LinkDemand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            try
            {
                if (_wrapped is ISerializable || _wrapped.GetType().IsSerializable)
                {
                    info.AddValue("wrappedtype", _wrapped.GetType());
                    info.AddValue("wrappedvalue", _wrapped);
                }
                else
                {
                    info.AddValue("wrappedtype", typeof(string));
                    string serializedString = string.Empty;
                    try
                    {
                        serializedString = _wrapped?.ToString();
                    }
                    finally
                    {
                        info.AddValue("wrappedvalue", serializedString ?? string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                // ToString on random object can throw exception
                InternalLogger.Debug(ex, "ObjectHandleSerializer failed to serialize object: {0}", _wrapped?.GetType());
            }
        }

        public object Unwrap()
        {
            return _wrapped ?? string.Empty;
        }
    }
#endif
}