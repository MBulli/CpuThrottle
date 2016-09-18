using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CpuThrottle
{
    class Program
    {
        static void Main(string[] args)
        {
            //FreeConsole();

            if (args.Length == 0)
            {
                SetCpuSpeed(50);

                // Start watchdog
                Process.Start(typeof(Program).Assembly.Location, Process.GetCurrentProcess().Id.ToString());

                Console.WriteLine("Waiting for termination...");
                Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                // we are the watchdog
                int procID = int.Parse(args[0]);
                var procToWatch = Process.GetProcessById(procID);
                procToWatch.WaitForExit();

                SetCpuSpeed(100);
            }
        }

        static void SetCpuSpeed(int level)
        {
            const string GUID_TYPICAL_POWER_SAVINGS = "381b4222-f694-41f0-9685-ff5bb260df2e"; // aka Balanced
            const string GUID_PROCESSOR_SETTINGS_SUBGROUP = "54533251-82be-4824-96c1-47b60b740d00";
            const string GUID_PROCESSOR_THROTTLE_MAXIMUM = "bc5038f7-23e0-4960-96da-33abaf5935ec";

            var activeScheme = GetActiveSchemeGuid();

            var arg = $@"-SETACVALUEINDEX {activeScheme} {GUID_PROCESSOR_SETTINGS_SUBGROUP} {GUID_PROCESSOR_THROTTLE_MAXIMUM} {level}";
            Process.Start("powercfg", arg).WaitForExit();
        }

        private static Guid GetActiveSchemeGuid()
        {
            IntPtr ptr_activeSchemePtr = IntPtr.Zero;

            if (PowerGetActiveScheme(IntPtr.Zero, ref ptr_activeSchemePtr) != 0)
                throw new Win32Exception();

            Guid activeScheme = Marshal.PtrToStructure<Guid>(ptr_activeSchemePtr);

            if (LocalFree(ptr_activeSchemePtr) != IntPtr.Zero)
                throw new Win32Exception();

            return activeScheme;
        }
        


        [DllImport("powrprof.dll", SetLastError=true)]
        static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, ref IntPtr ActivePolicyGuid);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LocalFree(IntPtr hMem);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();
    }
}
