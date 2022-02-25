using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoDeleteProShared
{
    public class Configuration
    {
        public bool HologramEnabled { get; set; }
        public int HologramKey { get; set; }
        public bool HologramVisibleInVehicle { get; set; }
        public float HologramDistance { get; set; }
        public int TimeToLive { get; set; }
        public int TimeForUpdate { get; set; }
        public bool Debug { get; set; }
        public bool SkipOneSyncCheck { get; set; }
        public List<string> Blacklist { get; set; }
        public Dictionary<string, int> Custom { get; set; }
    }
}
