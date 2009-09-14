using System;
using System.Runtime.InteropServices;
using System.Security;

#if !SILVERLIGHT

namespace NLog.Internal
{
    internal static class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetConsoleTextAttribute(IntPtr hConsole, ushort wAtributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FillConsoleOutputAttribute(IntPtr hConsole, ushort wAttributes, int nLength, COORD dwWriteCoord, out uint written);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsole, out CONSOLE_BUFFER_INFO bufferInfo);

        internal const int STD_OUTPUT_HANDLE = -11;
        internal const int STD_ERROR_HANDLE = -12;

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int type);

        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            public ushort x;
            public ushort y;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SMALL_RECT
        {
            public ushort Left;
            public ushort Top;
            public ushort Right;
            public ushort Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CONSOLE_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

        // obtains user token
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LogonUser(string pszUsername, string pszDomain, string pszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        // closes open handes returned by LogonUser
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr handle);

        // creates duplicate token handle
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DuplicateToken(IntPtr existingTokenHandle, int impersonationLevel, out IntPtr duplicateTokenHandle);

        [DllImport("kernel32.dll")]
        internal static extern void OutputDebugString(string message);

#if !NET_CF
        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
#else
        [DllImport("coredll.dll")]
#endif
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryPerformanceCounter(out ulong lpPerformanceCount);

#if !NET_CF
        [DllImport("kernel32.dll"), SuppressUnmanagedCodeSecurity]
#else
        [DllImport("coredll.dll")]
#endif
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryPerformanceFrequency(out ulong lpPerformanceFrequency);
    }
}

#endif