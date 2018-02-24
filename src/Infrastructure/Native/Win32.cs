using System;
using System.Runtime.InteropServices;

namespace AIMP.DiskCover.Infrastructure.Native
{
    public static class Win32
    {
        [DllImport("user32", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, String lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
    }
}