# AutoDeletePro

AutoDeletePro adds a timer to vehicles for deletion. It does require OneSync Infinity.

## Features
* Configurable number of minutes until a vehicle is deleted after use
* Blacklist specific vehicles
* Set a specific time for specific vehicle models
* A hologram over the vehicle with a timer on which the time can be seen until the vehicle is deleted
* Hologram can also be set to only display when a key is held down (it will turn back off when the key is no longer pressed)

## Configuration JSON

The below example is consumer-ready.  It does have a "Hologram" as per requirements, and allows configuration to determine whether the hologram is always visible or only on key press.

This configuration will show the hologram for vehicles within 10 game units of the player, when they're on foot, and while holding the INPUT_CHARACTER_WHEEL action (def: Left Alt).  The vehicles 
will remain for 60 seconds with no one in them. Timer only begins when a player enters a vehicle and is reset when they enter the vehicle and periodically within.

Script can also handle a list of vehicles to never delete (blacklist), or apply custom times to specific vehicle models (custom).

Filename: config.json

```json
{
  "HologramEnabled": true,           // If true show timer over vehicle til auto deleted
  "HologramKey": 19,                 // Key to show hologram, if 0 will show within distance
  "HologramDistance": 10.0,          // Distance to show hologram if key not configured
  "HologramVisibleInVehicle": false, // Show hologram while in vehicle?
  "TimeToLive": 60,                  // Time for a vehicle to be allowed to sit idle in seconds
  "TimeForUpdate": 30,               // Number of seconds between updates (an approximate)
  "Blacklist": ["police"],           // List of vehicle models to not track 
  "Custom": {                        // List of vehicle models and times to apply to that model
    "police2": 60,
    "sheriff": 15
  }
}
```

## External Script Integration

A vehicle's "last used" time can be touched by external scripts by triggering an event called AutoDeletePro:TouchVehicle with the network ID as an argument.

Example:
```CSharp
TriggerServerEvent("AutoDeletePro:TouchVehicle", NetworkGetNetworkIdFromEntity(vehicle))
```
