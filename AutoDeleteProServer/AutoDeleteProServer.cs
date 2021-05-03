using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using AutoDeleteProShared;

namespace AutoDeleteProServer
{
    public class AutoDeleteProServer : BaseScript
    {
        private Dictionary<int, int> vehicleList = new Dictionary<int, int>();
        private readonly Configuration config = JsonConvert.DeserializeObject<Configuration>(LoadResourceFile(GetCurrentResourceName(), "config.json"));

        public AutoDeleteProServer()
        {
            DebugLog("Starting AutoDeletePro");
            EventHandlers["AutoDeletePro:TouchVehicle"] += new Action<Player, int>(TouchVehicle);

            Tick += VehicleCleanup;
            Tick += CacheBroadcast;
        }

        private void TouchVehicle([FromSource]Player source, int netId)
        {
            vehicleList[netId] = Utils.getCurrentEpoch() + config.TimeToLive;
            DebugLog("Touching vehicle " + netId + ", new TTL " + vehicleList[netId]);
        }

        private async Task VehicleCleanup()
        {
            await BaseScript.Delay(1000);

            int now = Utils.getCurrentEpoch();

            for (int i = 0; i < vehicleList.Count; i++)
            {
                if (now >= vehicleList[vehicleList.Keys.ElementAt(i)])
                {
                    Entity v = Entity.FromNetworkId(vehicleList.Keys.ElementAt(i));

                    if (v.Owner != null)
                    {
                        DebugLog("Sending event to delete vehicle " + v.NetworkId + " to: " + v.Owner.Handle);
                        v.Owner.TriggerEvent("AutoDeletePro:DeleteVehicle", vehicleList.Keys.ElementAt(i));
                    }
                    else
                    {
                        DebugLog("Deleting vehicle " + v.NetworkId + " from server.");
                        DeleteEntity(v.Handle);
                    }
                    vehicleList.Remove(vehicleList.Keys.ElementAt(i));
                }
            }
        }

        private async Task CacheBroadcast()
        {
            await BaseScript.Delay(10000);

            Debug.WriteLine("Sending cache update: " + JsonConvert.SerializeObject(vehicleList));
            TriggerClientEvent("AutoDeletePro:CacheUpdate", JsonConvert.SerializeObject(vehicleList));
        }

        private void DebugLog(string text)
        {
            if (config.Debug) Debug.WriteLine(text);
        }
    }
}
