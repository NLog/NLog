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

namespace NLog.Internal
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Reflection helpers.
    /// </summary>
    internal static class ReflectionHelpers
    {
        public static Assembly GetAssembly(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.Assembly;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;
#endif
        }

        public static Action<object, object> CreatePropertySetter(this PropertyInfo propertyInfo)
        {
            var target = System.Linq.Expressions.Expression.Parameter(typeof(object), "target");
            var value = System.Linq.Expressions.Expression.Parameter(typeof(object), "parameter");
            var body = System.Linq.Expressions.Expression.Call(System.Linq.Expressions.Expression.Convert(target, propertyInfo.DeclaringType), propertyInfo.GetSetMethod(true), System.Linq.Expressions.Expression.Convert(value, propertyInfo.PropertyType));
            return System.Linq.Expressions.Expression.Lambda<Action<object, object>>(body, target, value).Compile();
        }
    }
}
