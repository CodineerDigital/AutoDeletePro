# Configuration

## Sample

```json
{
  "HologramEnabled": false,          // If true show timer over vehicle til auto deleted
  "HologramKey": 0,                  // Key to show hologram, if 0 will show within distance
  "HologramDistance": 5.0,           // Distance to show hologram if key not configured
  "HologramVisibleInVehicle": false, // Show hologram while in vehicle?
  "TimeToLive": 60,                  // Time for a vehicle to be allowed to sit idle in seconds
  "TimeForUpdate": 30,               // Number of seconds between updates (an approximate)
  "Debug": false,                    // Shouldn't be exposed to consumers, but can be used to debug issues
  "Blacklist": ["police"],           // Vehicle models to not track 
  "Custom": {                        // List of vehicle models and times to apply to that model
    "police2": 60,
    "sheriff": 15
  }
}
```