using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PauseDropboxOnBattery
{
    internal class Program
    {
        private static readonly TimeSpan CheckIfOnBatteryInterval = TimeSpan.FromSeconds(5);
        private static readonly CancellationTokenSource ShouldAppQuit = new CancellationTokenSource();

        /// <summary>
        /// Checks if the device is currently using battery power. If there isn't a battery or the battery's status is unknown,
        /// then it returns false.
        /// </summary>
        private static bool IsOnBatteryPower
        {
            get
            {
                try
                {
                    var powerState = PowerState.GetPowerState();
                    var shouldSuspend = powerState.ACLineStatus.Equals(AcLineStatus.Offline) &&
                                        // do not suspend if a desktop computer
                                        powerState.BatteryFlag != BatteryFlag.NoSystemBattery &&
                                        powerState.BatteryFlag != BatteryFlag.Unknown;
                    return shouldSuspend;
                }
                catch (ApplicationException e)
                {
                    Console.WriteLine($"Error: not suspending Dropbox because {e.Message}");
                    return false;
                }
            }
        }

        private static async Task Main()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => ShouldAppQuit.Cancel();
            while (!ShouldAppQuit.IsCancellationRequested)
            {
                Console.WriteLine($"Should suspend Dropbox? {IsOnBatteryPower}");
                SetDropboxSuspension(IsOnBatteryPower);
                await Task.Delay(CheckIfOnBatteryInterval);
            }

            SetDropboxSuspension(false);
        }

        /// <summary>
        /// Conditionally suspends Dropbox
        /// </summary>
        /// <param name="shouldSuspend">Whether to suspend</param>
        private static void SetDropboxSuspension(bool shouldSuspend)
        {
            var dropboxProcesses =
                Process.GetProcesses().Where(process => process.ProcessName.Equals("Dropbox")).ToList();

            foreach (var process in dropboxProcesses)
                if (shouldSuspend)
                    ProcessUtils.SuspendProcess(process.Id);
                else
                    ProcessUtils.ResumeProcess(process.Id);
        }
    }
}