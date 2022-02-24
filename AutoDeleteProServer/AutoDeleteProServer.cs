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
        private Dictionary<int, int> customTimes = new Dictionary<int, int>();
        private List<int> blacklist = new List<int>();
        private readonly Configuration config = JsonConvert.DeserializeObject<Configuration>(LoadResourceFile(GetCurrentResourceName(), "config.json"));

        public AutoDeleteProServer()
        {
            Log("Starting script...");
            if (GetConvar("onesync_enableInfinity", "off") != "on")
            {
                Log("This script requires use of OneSync Infinity. Please ensure you are using OneSync Infinity and try again.");
            }
            else
            {
                EventHandlers["AutoDeletePro:TouchVehicle"] += new Action<Player, int, int>(TouchVehicle);
                EventHandlers["entityRemoved"] += new Action<int>(EntityRemoved);

                DebugLog("[AutoDeletePro] Building Custom Times.");
                foreach (KeyValuePair<string, int> entry in config.Custom)
                {
                    customTimes[GetHashKey(entry.Key)] = entry.Value;
                }

                DebugLog("[AutoDeletePro] Building Blacklist");
                foreach (string model in config.Blacklist)
                {
                    blacklist.Add(GetHashKey(model));
                }

                DebugLog("Done.");

                Tick += VehicleCleanup;
                Tick += CacheBroadcast;
                Log("Startup Complete.");
            }
        }

        private void TouchVehicle([FromSource]Player source, int netId, int hash)
        {
            DebugLog("TouchVehicle " + netId + ", hash: " + hash + " from existing " + JsonConvert.SerializeObject(vehicleList));
            Entity e = Entity.FromNetworkId(netId);
            DebugLog("Checking blacklist");
            if (!blacklist.Contains(hash))
            {
                DebugLog("Vehicle is not blacklisted");
                if (customTimes.ContainsKey(hash))
                {
                    DebugLog("Setting custom time");
                    vehicleList[netId] = Utils.getCurrentEpoch() + customTimes[hash];
                    DebugLog("Time set.");
                }
                else
                {
                    DebugLog("Setting standard time");
                    vehicleList[netId] = Utils.getCurrentEpoch() + config.TimeToLive;
                    DebugLog("Time set2.");
                }
                DebugLog("Touching vehicle " + netId + ", new TTL " + vehicleList[netId]);
            } else
            {
                DebugLog("Got touch vehicle " + netId + ", but is blacklisted.");
            }
        }

        private void EntityRemoved(int handle)
        {
            Entity e = Entity.FromHandle(handle);
            if (vehicleList.ContainsKey(e.NetworkId))
            {
                vehicleList.Remove(e.NetworkId);
            }
        }

        private async Task VehicleCleanup()
        {
            await BaseScript.Delay(1000);

            int now = Utils.getCurrentEpoch();

            for (int i = 0; i < vehicleList.Count; i++)
            {
                DebugLog("VehicleCleanup: " + i + " of " + vehicleList.Count);
                int key = vehicleList.Keys.ElementAt(i);
                DebugLog("Setting key " + key);
                if (now >= vehicleList[key])
                {
                    DebugLog("Now is past.");
                    Entity v = Entity.FromNetworkId(key);
                    DebugLog("Entity: " + v?.Handle);

                    if (v == null || (v != null && !DoesEntityExist(v.Handle)))
                    {
                        DebugLog("Entity doesn't exist, removing from list.");
                        vehicleList.Remove(key);
                        DebugLog("Key removed1.");
                    }
                    else
                    {
                        DebugLog("Entity is not null and does exist");
                        vehicleList.Remove(key);
                        DebugLog("Key removed.");
                        if (v?.Owner != null)
                        {
                            DebugLog("Sending event to delete vehicle " + v.NetworkId + " to: " + v.Owner.Handle);
                            v.Owner.TriggerEvent("AutoDeletePro:DeleteVehicle", key);
                        }
                        else
                        {
                            DebugLog("Deleting vehicle " + v.NetworkId + " from server.");
                            DeleteEntity(v.Handle);
                        }
                    }
                }
            }
        }

        private async Task CacheBroadcast()
        {
            await BaseScript.Delay(10000);

            DebugLog("Sending cache update: " + JsonConvert.SerializeObject(vehicleList));
            TriggerClientEvent("AutoDeletePro:CacheUpdate", JsonConvert.SerializeObject(vehicleList));
        }

        private void Log(string text)
        {
            Debug.WriteLine("[AutoDeletePro] " + text);
        }

        private void DebugLog(string text)
        {
            if (config.Debug) Log(text);
        }
    }
}
