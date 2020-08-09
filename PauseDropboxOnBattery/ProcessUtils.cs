using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PauseDropboxOnBattery
{
    /// <summary>
    ///     https://stackoverflow.com/questions/71257/suspend-process-in-c-sharp
    /// </summary>
    internal static class ProcessUtils
    {
        [Flags]
        public enum ThreadAccess
        {
            // ReSharper disable once InconsistentNaming
            SUSPEND_RESUME = 0x0002
        }

        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (string.IsNullOrEmpty(process.ProcessName))
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) pT.Id);

                if (pOpenThread == IntPtr.Zero) continue;

                int suspendCount;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        public static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            foreach (ProcessThread pT in process.Threads)
            {
                var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint) pT.Id);

                if (pOpenThread == IntPtr.Zero) continue;

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }
    }
}