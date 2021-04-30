using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace AutoDeleteProClient
{
    public class AutoDeleteProClient : BaseScript
    {


        public AutoDeleteProClient()
        {
            EventHandlers["AutoDeletePro:DeleteVehicle"] += new Action<int>(DeleteVehicle);

        }

        private void DeleteVehicle(int netId)
        {
            Entity e = Entity.FromNetworkId(netId);
            e.Delete();
        }
    }
}
