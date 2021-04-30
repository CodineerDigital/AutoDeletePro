using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace AutoDeleteProServer
{
    public class AutoDeleteProServer : BaseScript
    {
        private Dictionary<int, int> vehicleList = new Dictionary<int, int>();
        private readonly Configuration config = JsonConvert.DeserializeObject<Configuration>(LoadResourceFile(GetCurrentResourceName(), "config.json"));

        public AutoDeleteProServer()
        {
            EventHandlers["AutoDeletePro:TouchVehicle"] += new Action<Player, int>(TouchVehicle);

            Tick += VehicleCleanup;
        }

        private void TouchVehicle([FromSource]Player source, int netId)
        {
            vehicleList[netId] = AutoDeleteProShared.Utils.getCurrentEpoch() + config.TimeToLive;
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
