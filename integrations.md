# Integrations

## Introduction

The following event(s) can be used for external integrations with this script.

## Events

### AutoDeletePro:TouchVehicle

Argument: Network ID

This event is used to "touch" the time of a vehicle, which will reset the timer until deletion. This will simulate a person having entered the car when the event is received.

Example:
```CSharp
TriggerServerEvent("AutoDeletePro:TouchVehicle", NetworkGetNetworkIdFromEntity(vehicle))
```