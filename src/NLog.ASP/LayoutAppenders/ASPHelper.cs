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
