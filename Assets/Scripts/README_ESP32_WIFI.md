# ESP32 WiFi Pressure Sensor Integration

This guide explains how to use the ESP32WiFiManager to connect your ESP32C3 pressure sensor to Unity via WiFi.

## Quick Start

1. Add the ESP32WiFiManager component to a GameObject in your scene
2. Set the WebSocket URL to match your ESP32's IP address (e.g., `ws://192.168.1.100/ws`)
3. For testing without hardware, enable "Use Simulated Data"
4. Subscribe to the pressure data events in your game scripts

## Using ESP32WiFiManager in Your Scripts

```csharp
using UnityEngine;

public class MyGameController : MonoBehaviour
{
    [SerializeField] private ESP32WiFiManager wifiManager;
    
    void Start()
    {
        // If no WiFi manager is assigned, try to find one
        if (wifiManager == null)
        {
            wifiManager = FindObjectOfType<ESP32WiFiManager>();
            if (wifiManager == null)
            {
                Debug.LogError("No ESP32WiFiManager found in the scene!");
                return;
            }
        }
        
        // Subscribe to pressure data events
        wifiManager.OnPressureDataReceived += OnPressureDataReceived;
        
        Debug.Log("MyGameController initialized. Ready to receive pressure data.");
    }
    
    void OnPressureDataReceived(float pressure)
    {
        // Do something with the pressure value
        Debug.Log("Received pressure: " + pressure);
        
        // Example: Move an object based on pressure
        transform.position = new Vector3(0, pressure / 100f, 0);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from pressure data events
        if (wifiManager != null)
        {
            wifiManager.OnPressureDataReceived -= OnPressureDataReceived;
        }
    }
}
```

## ESP32WiFiManager Properties

| Property | Description |
|----------|-------------|
| webSocketUrl | The WebSocket URL of your ESP32 (e.g., `ws://192.168.1.100/ws`) |
| useSimulatedData | Enable to generate random pressure data for testing |
| simulatedMinPressure | Minimum value for simulated pressure (default: 0) |
| simulatedMaxPressure | Maximum value for simulated pressure (default: 100) |
| simulationUpdateInterval | How often to generate new simulated values (in seconds) |

## ESP32WiFiManager Events

| Event | Description |
|-------|-------------|
| OnPressureDataReceived | Triggered when new pressure data is received |

## Troubleshooting

### Cannot find ESP32WiFiManager component

If you're looking for the ESP32WiFiManager component in the Unity Editor:

1. Make sure you've imported the scripts correctly
2. In the Add Component menu, search for "ESP32WiFi" (not "ESP32WebSocket")
3. If you still can't find it, try restarting Unity

### Connection Issues

1. Verify your ESP32's IP address by checking the Serial Monitor in Arduino IDE
2. Make sure your computer and ESP32 are on the same network
3. Try pinging the ESP32's IP address to verify connectivity
4. Check if any firewalls are blocking the WebSocket connection

### Testing Without Hardware

1. Enable "Use Simulated Data" on the ESP32WiFiManager component
2. Adjust the simulation parameters as needed
3. You should see random pressure values being generated in the Unity console

## Switching Between Bluetooth and WiFi

If you want to switch between Bluetooth and WiFi connections:

1. In your PressureGameController, set the connectionType to either:
   - ConnectionType.Bluetooth
   - ConnectionType.WiFi

2. Both managers can exist in the same scene, and the controller will use the appropriate one based on the connectionType setting.