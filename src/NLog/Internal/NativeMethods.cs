using System;
using System.Text;
using System.Runtime.InteropServices;

namespace NLog.Internal
{
    internal class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetConsoleTextAttribute(IntPtr hConsole, ushort wAtributes);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FillConsoleOutputAttribute(IntPtr hConsole, ushort wAttributes, int nLength, COORD dwWriteCoord, out uint written);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetConsoleScreenBufferInfo(IntPtr hConsole, out CONSOLE_SCREEN_BUFFER_INFO bufferInfo);

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
        internal struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }
    }
}
