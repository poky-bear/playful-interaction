# ESP32C3 Pressure Sensor Integration

This guide explains how to set up an ESP32C3 with a pressure sensor and connect it to Unity using either WiFi (WebSockets) or Bluetooth.

## Hardware Requirements

- ESP32C3 development board
- Pressure sensor (analog output)
- Jumper wires
- Breadboard
- USB cable for programming

## Wiring the Pressure Sensor

1. Connect the pressure sensor to the ESP32C3:
   - VCC pin of the sensor to 3.3V on ESP32C3
   - GND pin of the sensor to GND on ESP32C3
   - Signal/Output pin of the sensor to A0 (analog input) on ESP32C3

## Software Setup

### Arduino IDE Setup

1. Install the Arduino IDE from [arduino.cc](https://www.arduino.cc/en/software)
2. Add ESP32 board support:
   - Open Arduino IDE
   - Go to File > Preferences
   - Add `https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json` to Additional Board Manager URLs
   - Go to Tools > Board > Boards Manager
   - Search for "esp32" and install the ESP32 package

3. Install required libraries:
   - Go to Tools > Manage Libraries
   - Search for and install:
     - `ArduinoJson`
     - `AsyncTCP` (for ESP32)
     - `ESPAsyncWebServer`

### WiFi Implementation (Recommended)

1. Open the `ESP32C3_PressureSensor_WiFi.ino` sketch
2. Update the WiFi credentials:
   ```cpp
   const char* ssid = "YourWiFiName";
   const char* password = "YourWiFiPassword";
   ```
3. Upload the sketch to your ESP32C3
4. Open the Serial Monitor (115200 baud) to see the IP address of your ESP32C3
5. Note the IP address - you'll need it for Unity

### Bluetooth Implementation (Alternative)

If you prefer using Bluetooth:

1. Open the `ESP32C3_PressureSensor.ino` sketch
2. Upload the sketch to your ESP32C3
3. Open the Serial Monitor (115200 baud) to verify it's working

## Unity Integration

### WiFi Setup (Recommended)

1. In your Unity scene, add an empty GameObject
2. Add the `ESP32WiFiManager` component to it
3. Set the WebSocket URL to `ws://YOUR_ESP32_IP_ADDRESS/ws` (replace with your ESP32's IP)
4. For testing, you can enable "Use Simulated Data" to generate random pressure values

### Bluetooth Setup (Alternative)

1. In your Unity scene, add an empty GameObject
2. Add the `ESP32BluetoothManager` component to it
3. For testing, you can enable "Use Simulated Data" to generate random pressure values

### Using the Pressure Data in Your Game

1. Create a new script that subscribes to the pressure data events:
   ```csharp
   // Get reference to the manager
   private ESP32WiFiManager wifiManager;
   
   void Start() {
       // Find the manager in the scene
       wifiManager = FindObjectOfType<ESP32WiFiManager>();
       
       // Subscribe to pressure data events
       wifiManager.OnPressureDataReceived += OnPressureDataReceived;
   }
   
   void OnPressureDataReceived(float pressure) {
       // Do something with the pressure value
       Debug.Log("Received pressure: " + pressure);
   }
   
   void OnDestroy() {
       // Unsubscribe when done
       if (wifiManager != null) {
           wifiManager.OnPressureDataReceived -= OnPressureDataReceived;
       }
   }
   ```

2. Use the pressure data to control game elements:
   - Map pressure to player movement
   - Use pressure thresholds to trigger actions
   - Control UI elements based on pressure

## Demo Scenes

1. **ESP32WiFiPressureDemo**: A simple scene demonstrating pressure visualization
   - Shows pressure value on a slider and text
   - Changes color based on pressure
   - Makes a cube jump when pressure exceeds threshold

2. **Ring Game**: A game where you control a ring with pressure
   - Try to keep the inner ring within the target zone
   - Score points by maintaining pressure at the right level

## Troubleshooting

### WiFi Connection Issues

- Verify the ESP32C3 is connected to your WiFi network
- Check the IP address in the Serial Monitor
- Make sure your Unity project is using the correct IP address
- Ensure your computer is on the same network as the ESP32C3

### Pressure Sensor Issues

- Check the wiring connections
- Verify the sensor is working by monitoring the raw values in the Serial Monitor
- Adjust the mapping range in the Arduino code if needed:
  ```cpp
  int mappedPressure = map(pressureValue, 0, 4095, 0, 100);
  ```

### Unity Integration Issues

- Check the Unity console for error messages
- Verify the WebSocket URL is correct
- Try enabling "Use Simulated Data" to test without the hardware