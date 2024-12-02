using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteryMonitor.Shared
{
    public static class BatteryUtility
    {
        public static int GetBatteryLevel() => (int)(Battery.ChargeLevel * 100);

        public static BatteryState GetBatteryStatus() => Battery.State;
    }
}
