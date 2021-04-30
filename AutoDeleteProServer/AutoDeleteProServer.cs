using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace AutoDeleteProServer
{
    public class AutoDeleteProServer : BaseScript
    {
        private Dictionary<int, int> vehicleList = new Dictionary<int, int>();

        public AutoDeleteProServer()
        {
            EventHandlers["AutoDeletePro:TouchVehicle"] += new Action<Player, int>(TouchVehicle);

            Tick += VehicleCleanup;
        }

        private void TouchVehicle([FromSource]Player source, int netId)
        {
            vehicleList[netId] = AutoDeleteProShared.Utils.getCurrentEpoch();
        }

        private async Task VehicleCleanup()
        {
            await Delay(1000);

            int now = AutoDeleteProShared.Utils.getCurrentEpoch();

            for (int i = 0; i < vehicleList.Count; i++)
            {
                if (now >= vehicleList[vehicleList.Keys.ElementAt(i)])
                {
                    Entity v = Entity.FromNetworkId(vehicleList.Keys.ElementAt(i));

                    if (v.Owner != null)
                    {
                        v.Owner.TriggerEvent("AutoDeletePro:DeleteVehicle", vehicleList.Keys.ElementAt(i));
                    }
                    else
                    {
                        DeleteEntity(v.Handle);
                    }
                }
            }
        }
    }
}
