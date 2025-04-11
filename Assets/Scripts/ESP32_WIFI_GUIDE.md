# ESP32 WiFi Pressure Sensor Integration Guide

This guide provides detailed instructions for setting up and using the ESP32WiFiManager to connect your ESP32C3 pressure sensor to Unity via WiFi.

## Table of Contents

1. [Overview](#overview)
2. [Unity Setup](#unity-setup)
3. [ESP32 Arduino Setup](#esp32-arduino-setup)
4. [Testing the Connection](#testing-the-connection)
5. [Integrating with Your Game](#integrating-with-your-game)
6. [Troubleshooting](#troubleshooting)

## Overview

The ESP32WiFiManager provides a simple way to connect your ESP32C3 pressure sensor to Unity using WiFi and WebSockets. This approach offers several advantages over Bluetooth:

- Longer range
- More stable connection
- No pairing required
- Works on all platforms (including WebGL)

## Unity Setup

### 1. Add the ESP32WiFiManager to Your Scene

1. Create an empty GameObject in your scene
2. Add the ESP32WiFiManager component to it
   - In the Inspector, click "Add Component"
   - Search for "ESP32WiFi"
   - Select "ESP32WiFiManager"

### 2. Configure the ESP32WiFiManager

Set the following properties in the Inspector:

- **WebSocket URL**: The WebSocket URL of your ESP32 (e.g., `ws://192.168.1.100/ws`)
- **Use Simulated Data**: Enable for testing without hardware
- **Simulated Min/Max Pressure**: Range for simulated pressure values
- **Simulation Update Interval**: How often to generate new simulated values

### 3. Create a Script to Use the Pressure Data

Create a new C# script that subscribes to the pressure data events:

```csharp
using UnityEngine;

public class PressureController : MonoBehaviour
{
    [SerializeField] private ESP32WiFiManager wifiManager;
    [SerializeField] private float pressureThreshold = 50f;
    
    private bool isPressed = false;
    
    void Start()
    {
        // Find the WiFi manager if not assigned
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
    }
    
    void OnPressureDataReceived(float pressure)
    {
        // Check if pressure exceeds threshold
        if (pressure >= pressureThreshold && !isPressed)
        {
            isPressed = true;
            OnPressureDown();
        }
        else if (pressure < pressureThreshold && isPressed)
        {
            isPressed = false;
            OnPressureUp();
        }
        
        // You can also use the continuous pressure value
        OnPressureChanged(pressure);
    }
    
    void OnPressureDown()
    {
        Debug.Log("Pressure Down!");
        // Add your pressure down logic here
    }
    
    void OnPressureUp()
    {
        Debug.Log("Pressure Up!");
        // Add your pressure up logic here
    }
    
    void OnPressureChanged(float pressure)
    {
        // Add your continuous pressure logic here
        // For example, scale an object based on pressure
        transform.localScale = Vector3.one * (1f + pressure / 100f);
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

## ESP32 Arduino Setup

### 1. Install Required Libraries

In the Arduino IDE, install the following libraries:

- WiFi (included with ESP32 board)
- WebSockets by Markus Sattler (ArduinoWebsockets)
- ArduinoJson

### 2. Upload the ESP32 Sketch

```cpp
#include <WiFi.h>
#include <ArduinoWebsockets.h>
#include <ArduinoJson.h>

// WiFi credentials
const char* ssid = "YOUR_WIFI_SSID";
const char* password = "YOUR_WIFI_PASSWORD";

// WebSocket server
using namespace websockets;
WebsocketsServer server;

// Pressure sensor pin
const int pressureSensorPin = A0;

// Calibration values
const float minPressure = 0.0;
const float maxPressure = 1023.0;

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  
  // Connect to WiFi
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(1000);
    Serial.println("Connecting to WiFi...");
  }
  
  // Print IP address
  Serial.print("Connected to WiFi. IP address: ");
  Serial.println(WiFi.localIP());
  
  // Start WebSocket server
  server.listen(80);
  Serial.println("WebSocket server started on port 80");
  Serial.println("WebSocket URL: ws://" + WiFi.localIP().toString() + "/ws");
}

void loop() {
  // Look for WebSocket connections
  WebsocketsClient client = server.accept();
  
  if (client.available()) {
    Serial.println("Client connected");
    
    // Keep connection alive until client disconnects
    while (client.available()) {
      // Read pressure sensor
      int rawPressure = analogRead(pressureSensorPin);
      
      // Map to calibrated range
      float pressure = map(rawPressure, minPressure, maxPressure, 0, 100);
      
      // Create JSON message
      StaticJsonDocument<64> doc;
      doc["pressure"] = pressure;
      doc["timestamp"] = millis();
      
      // Serialize JSON to string
      String jsonString;
      serializeJson(doc, jsonString);
      
      // Send data to client
      client.send(jsonString);
      
      // Wait a bit before sending next reading
      delay(100);
    }
    
    Serial.println("Client disconnected");
  }
  
  // Small delay to prevent CPU hogging
  delay(10);
}
```

### 3. Get the ESP32's IP Address

After uploading the sketch, open the Serial Monitor to see the ESP32's IP address. You'll need this for the WebSocket URL in Unity.

## Testing the Connection

### 1. Use the ESP32WiFiPressureDemo Scene

1. Open the ESP32WiFiDemo scene
2. Enter your ESP32's IP address in the WebSocket URL field
3. Disable "Use Simulated Data" if you have the hardware connected
4. Play the scene
5. You should see pressure values being displayed in the console

### 2. Test with Simulated Data

If you don't have the hardware yet:

1. Enable "Use Simulated Data"
2. Adjust the simulation parameters as needed
3. Play the scene
4. You should see random pressure values being generated

## Integrating with Your Game

### 1. Add the ESP32WiFiManager to Your Game Scene

1. Add the ESP32WiFiManager component to a GameObject in your game scene
2. Configure it with your ESP32's IP address

### 2. Create a Controller Script

Create a script that uses the pressure data to control your game:

```csharp
using UnityEngine;

public class PressureGameController : MonoBehaviour
{
    [SerializeField] private ESP32WiFiManager wifiManager;
    [SerializeField] private float pressureThreshold = 50f;
    
    // Game-specific variables
    [SerializeField] private GameObject player;
    [SerializeField] private float jumpForce = 10f;
    
    private Rigidbody playerRb;
    private bool canJump = true;
    
    void Start()
    {
        // Find the WiFi manager if not assigned
        if (wifiManager == null)
        {
            wifiManager = FindObjectOfType<ESP32WiFiManager>();
        }
        
        // Get player components
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
        
        // Subscribe to pressure data events
        if (wifiManager != null)
        {
            wifiManager.OnPressureDataReceived += OnPressureDataReceived;
        }
        else
        {
            Debug.LogError("No ESP32WiFiManager found!");
        }
    }
    
    void OnPressureDataReceived(float pressure)
    {
        // Example: Jump when pressure exceeds threshold
        if (pressure >= pressureThreshold && canJump && playerRb != null)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            canJump = false;
            Invoke("ResetJump", 1f);
        }
        
        // Example: Scale player based on pressure
        if (player != null)
        {
            float scale = 1f + (pressure / 200f); // Max 1.5x size at 100 pressure
            player.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    
    void ResetJump()
    {
        canJump = true;
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

## Troubleshooting

### Connection Issues

1. **Cannot connect to ESP32**
   - Verify the ESP32's IP address in the Serial Monitor
   - Make sure your computer and ESP32 are on the same network
   - Try pinging the ESP32's IP address to verify connectivity
   - Check if any firewalls are blocking the WebSocket connection

2. **Connection drops frequently**
   - Ensure the ESP32 has a stable power source
   - Move the ESP32 closer to the WiFi router
   - Try a different WiFi network
   - Reduce the data sending frequency in the Arduino sketch

### Unity Issues

1. **Cannot find ESP32WiFiManager component**
   - Make sure you've imported the scripts correctly
   - In the Add Component menu, search for "ESP32WiFi" (not "ESP32WebSocket")
   - If you still can't find it, try restarting Unity

2. **No pressure data received**
   - Check the Unity console for error messages
   - Verify the WebSocket URL is correct
   - Try enabling "Use Simulated Data" to test if the component works

### Pressure Sensor Issues

1. **Erratic pressure readings**
   - Check the wiring of your pressure sensor
   - Adjust the calibration values in the Arduino sketch
   - Add smoothing to the pressure readings:
     ```cpp
     // Add at the top of your Arduino sketch
     const int numReadings = 10;
     float readings[numReadings];
     int readIndex = 0;
     float total = 0;
     
     // In loop(), replace the pressure reading with:
     int rawPressure = analogRead(pressureSensorPin);
     total = total - readings[readIndex];
     readings[readIndex] = rawPressure;
     total = total + readings[readIndex];
     readIndex = (readIndex + 1) % numReadings;
     float smoothedPressure = total / numReadings;
     float pressure = map(smoothedPressure, minPressure, maxPressure, 0, 100);
     ```

2. **Pressure values out of range**
   - Adjust the `minPressure` and `maxPressure` values in the Arduino sketch
   - Use the Serial Monitor to see the raw pressure values and adjust accordingly