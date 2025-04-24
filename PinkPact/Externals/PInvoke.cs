using System.Runtime.InteropServices;
using System.Text;
using System;

namespace PinkPact
{
    internal class PInvoke
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetShortPathName(string lpszLongPath, StringBuilder lpszShortPath, uint cchBuffer);

        [DllImport("winmm.dll")]
        public static extern int mciSendString(string command, StringBuilder buffer, int size, IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int virtKey);

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte virtKey, byte virtScan, uint flags, int info);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint code, uint type);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

        [DllImport("Shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, uint dpiType, out uint dpiX, out uint dpiY);
    }
}
