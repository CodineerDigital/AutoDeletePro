using AutoDeleteProShared;
using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace AutoDeleteProClient
{
    public class AutoDeleteProClient : BaseScript
    {
        private Dictionary<int, int> vehicleCache = new Dictionary<int, int>();
        private List<int> blacklist = new List<int>();
        private readonly Configuration config = JsonConvert.DeserializeObject<Configuration>(LoadResourceFile(GetCurrentResourceName(), "config.json"));
        private Vehicle lastVehicle = null;
        private int lastUpdate;

        public AutoDeleteProClient()
        {
            DebugLog("Starting script");

            EventHandlers["AutoDeletePro:DeleteVehicle"] += new Action<int>(DeleteVehicle);
            EventHandlers["AutoDeletePro:CacheUpdate"] += new Action<string>(CacheUpdate);

            // Create cache of hashes
            foreach(string model in config.Blacklist)
            {
                blacklist.Add(GetHashKey(model));
            }

            if (config.HologramEnabled)
            {
                Tick += HologramTick;
            }
            Tick += VehicleCheck;
        }

        private async Task VehicleCheck()
        {
            await Delay(0);
            if (Game.PlayerPed.IsInVehicle() && lastVehicle != Game.PlayerPed.CurrentVehicle)
            {
                if (!IsBlacklisted(Game.PlayerPed.CurrentVehicle))
                {
                    DebugLog("In vehicle and last vehicle doesn't match, updating, vehicle: " + Game.PlayerPed.CurrentVehicle.NetworkId);
                    lastVehicle = Game.PlayerPed.CurrentVehicle;
                    TriggerServerEvent("AutoDeletePro:TouchVehicle", Game.PlayerPed.CurrentVehicle.NetworkId, Game.PlayerPed.CurrentVehicle.Model.Hash);
                    lastUpdate = Game.GameTime;
                } else
                {
                    DebugLog("Player is in blacklisted vehicle.");
                }
            }
            else if (!Game.PlayerPed.IsInVehicle() && lastVehicle != null)
            {
                if (lastVehicle.Exists())
                {
                    DebugLog("No longer in vehicle, touching last vehicle: " + lastVehicle.NetworkId);
                    TriggerServerEvent("AutoDeletePro:TouchVehicle", lastVehicle.NetworkId, lastVehicle.Model.Hash);
                }
                lastVehicle = null;
            }
            else
            {
                if (lastVehicle != null && Game.GameTime > lastUpdate + config.TimeForUpdate * 1000)
                {
                    DebugLog("Still in vehicle, touching it: " + lastVehicle.NetworkId);
                    TriggerServerEvent("AutoDeletePro:TouchVehicle", lastVehicle.NetworkId, lastVehicle.Model.Hash);
                    lastUpdate = Game.GameTime;
                }
            }
        }

        private void DebugLog(string text)
        {
            if (config.Debug) Debug.WriteLine(text);
        }

        private async Task HologramTick()
        {
            await Task.FromResult(0);

            if (!config.HologramVisibleInVehicle && Game.PlayerPed.IsInVehicle())
            {
                return;
            }

            // GetLastInputMethod to ensure it's keyboard only https://forum.cfx.re/t/how-to-disable-controller-input-for-scripts/182364/4
            if ((config.HologramKey == 0 || (IsControlPressed(0, config.HologramKey) && GetLastInputMethod(0))))
            {
                List<Vehicle> vehicles = new List<Vehicle>();
                vehicles = World.GetAllVehicles().Where(v => v.IsOnScreen && v.Position.DistanceToSquared(Game.PlayerPed.Position) < config.HologramDistance).ToList();

                foreach(Vehicle v in vehicles)
                {
                    if (vehicleCache.ContainsKey(v.NetworkId) && v.NetworkId != Game.PlayerPed.CurrentVehicle?.NetworkId)
                    {
                        SetDrawOrigin(v.Position.X, v.Position.Y, v.Position.Z, 0);
                        DrawTextOnScreen(PrettifyTime(vehicleCache[v.NetworkId]), 0f, 0f, 0.3f, CitizenFX.Core.UI.Alignment.Center, 0, false);
                    }
                }
            }
        }

        private bool IsBlacklisted(Vehicle vehicle)
        {
            if (blacklist.Contains(vehicle.Model.Hash))
            {
                return true;
            }
            return false;
        }

        private string PrettifyTime(int deletion)
        {
            int diff = deletion - Utils.getCurrentEpoch();
            if (diff < 0) { return "Expired"; }

            int hours = diff / 3600;
            int mins = (diff % 3600) / 60;
            int secs = (diff % 60);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);
        }

        private void DeleteVehicle(int netId)
        {
            Entity e = Entity.FromNetworkId(netId);
            DebugLog("Got request to delete vehicle: " + netId);
            if (e != null) e.Delete();
        }

        private void CacheUpdate(string cache)
        {
            vehicleCache = JsonConvert.DeserializeObject<Dictionary<int, int>>(cache);
            DebugLog("Cache updated: " + cache);
        }

        private void DrawTextOnScreen(string text, float x, float y, float size, CitizenFX.Core.UI.Alignment justification, int font, bool disableTextOutline)
        {
            if (!IsHudHidden() && !IsPauseMenuActive())
            {
                SetTextFont(font);
                SetTextScale(1.0f, size);
                if (justification == CitizenFX.Core.UI.Alignment.Right)
                {
                    SetTextWrap(0f, x);
                }

                SetTextJustification((int)justification);
                if (!disableTextOutline) { SetTextOutline(); }
                BeginTextCommandDisplayText("STRING");
                AddTextComponentSubstringPlayerName(text);
                EndTextCommandDisplayText(x, y);
            }
        }
    }
}
