// 
// Copyright (c) 2004 Jaroslaw Kowalski <jaak@polbox.com>
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
// * Neither the name of the Jaroslaw Kowalski nor the names of its 
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
using System.Runtime.InteropServices;

namespace NLog.ASP.LayoutAppenders
{
    internal class ASPHelper
    {
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("51372ae0-cae7-11cf-be81-00aa00a2fa25")]
        public interface IObjectContext
        {
            // members not important
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("51372af4-cae7-11cf-be81-00aa00a2fa25")]
        public interface IGetContextProperties
        {
            int Count();
            object GetProperty(string name);
            // EnumNames omitted
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("D97A6DA0-A865-11cf-83AF-00A0C90C2BD8")]
        public interface ISessionObject
        {
            string GetSessionID();
            object GetValue(string name);
            void PutValue(string name, object val);
            int GetTimeout();
            void PutTimeout(int t);
            void Abandon();
            int GetCodePage();
            void PutCodePage(int cp);
            int GetLCID();
            void PutLCID();
            // GetStaticObjects
            // GetContents
        }

        [DllImport("ole32.dll")]
        extern static void CoGetObjectContext(ref Guid iid, out IObjectContext g);
        
        static Guid IID_IObjectContext = new Guid("51372ae0-cae7-11cf-be81-00aa00a2fa25");

        public static object GetSessionValue(string name)
        {
            IObjectContext obj;
            CoGetObjectContext(ref IID_IObjectContext, out obj);
            IGetContextProperties prop = (IGetContextProperties)obj;
            ISessionObject session = (ISessionObject)prop.GetProperty("Session");
            object retVal = session.GetValue(name);
            Marshal.ReleaseComObject(session);
            Marshal.ReleaseComObject(prop);
            Marshal.ReleaseComObject(obj);
            return retVal;
        }

    }
}
