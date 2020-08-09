using System;
using System.Runtime.InteropServices;

namespace PauseDropboxOnBattery
{
    [StructLayout(LayoutKind.Sequential)]
    public class PowerState
    {
        public AcLineStatus ACLineStatus;
        public BatteryFlag BatteryFlag;
        public int BatteryFullLifeTime;
        public byte BatteryLifePercent;
        public int BatteryLifeTime;
        public byte Reserved1;

        public static PowerState GetPowerState()
        {
            var state = new PowerState();
            if (GetSystemPowerStatusRef(state))
                return state;

            throw new ApplicationException("Unable to get power state");
        }

        [DllImport("Kernel32", EntryPoint = "GetSystemPowerStatus")]
        private static extern bool GetSystemPowerStatusRef(PowerState sps);
    }
}