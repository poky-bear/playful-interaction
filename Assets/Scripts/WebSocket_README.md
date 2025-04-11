# ESP32C3 Pressure Sensor WebSocket Integration

This document explains how to set up and use the WebSocket-based pressure sensor integration with Unity.

## Overview

This implementation uses WebSockets to communicate between an ESP32C3 microcontroller with a pressure sensor and a Unity game. The ESP32C3 hosts a WebSocket server that sends pressure data in JSON format, and Unity connects as a client to receive this data.

## Unity Setup

1. **Import the WebSocket Library**:
   - The NativeWebSocket library is included in the `Assets/Plugins/NativeWebSocket` folder
   - No additional imports are needed

2. **Add the ESP32WebSocketManager to your scene**:
   - Create an empty GameObject in your scene
   - Add the `ESP32WebSocketManager` component to it
   - Configure the WebSocket URL to match your ESP32C3's IP address (e.g., `ws://192.168.1.100/ws`)
   - For testing without the ESP32C3 hardware, keep the `useSimulatedData` option enabled

3. **Use the WebSocketPressureGameController**:
   - Either add the `WebSocketPressureGameController` component to your game object
   - Or use the `WebSocketRingGameSetup` script to create a complete ring game UI

4. **Testing**:
   - Play the scene in the Unity Editor
   - The ESP32WebSocketManager will generate simulated pressure data if `useSimulatedData` is enabled
   - Press the space bar to simulate pressure input manually

## ESP32C3 Setup

1. **Required Libraries**:
   - Install the following libraries in the Arduino IDE:
     - ESP32 board support
     - ESPAsyncWebServer
     - AsyncTCP
     - ArduinoJson

2. **Hardware Setup**:
   - Connect your pressure sensor to the ESP32C3's analog input pin (A0 by default)
   - Connect an LED to pin 2 for status indication (optional)

3. **Configure the Code**:
   - Open the `Arduino/ESP32C3_PressureSensor_WebSocket/ESP32C3_PressureSensor_WebSocket.ino` file
   - Update the WiFi credentials with your network details:
     ```cpp
     const char* ssid = "YourWiFiName";
     const char* password = "YourWiFiPassword";
     ```
   - Adjust the pressure sensor mapping if needed:
     ```cpp
     int mappedPressure = map(pressureValue, 0, 4095, 0, 100);
     ```

4. **Upload the Code**:
   - Connect your ESP32C3 to your computer
   - Select the correct board and port in the Arduino IDE
   - Upload the sketch

5. **Find the IP Address**:
   - Open the Serial Monitor (115200 baud)
   - The ESP32C3 will print its IP address after connecting to WiFi
   - Note this IP address for the Unity configuration

## Connecting Unity to ESP32C3

1. **Update the WebSocket URL**:
   - In the Unity Inspector, select the GameObject with the ESP32WebSocketManager
   - Set the `webSocketUrl` to `ws://YOUR_ESP32_IP_ADDRESS/ws`
   - Disable the `useSimulatedData` option

2. **Test the Connection**:
   - Play the scene in Unity
   - Apply pressure to the sensor
   - You should see pressure values being logged in the Unity console
   - The game should respond to pressure input

## Troubleshooting

1. **Connection Issues**:
   - Ensure Unity and ESP32C3 are on the same network
   - Check firewall settings that might block WebSocket connections
   - Verify the IP address is correct
   - Try accessing the ESP32C3's web interface in a browser (http://YOUR_ESP32_IP_ADDRESS)

2. **Pressure Sensor Calibration**:
   - If the pressure values are not in the expected range, adjust the mapping in the Arduino code
   - You may need to modify the `pressureThreshold` in the WebSocketPressureGameController

3. **Performance Issues**:
   - If you experience lag, try reducing the data send rate in the Arduino code
   - Adjust the `sendInterval` value to send data less frequently

## Advanced Configuration

1. **ESP32C3 as Access Point**:
   - If you want the ESP32C3 to create its own WiFi network, replace the WiFi connection code with:
     ```cpp
     WiFi.softAP("ESP32_PressureSensor", "password");
     Serial.println("Access Point Started");
     Serial.print("IP Address: ");
     Serial.println(WiFi.softAPIP());
     ```

2. **Multiple Pressure Sensors**:
   - To use multiple sensors, modify the Arduino code to read from additional pins
   - Update the JSON structure to include multiple pressure values
   - Modify the Unity code to parse the additional values

3. **Secure WebSockets**:
   - For production use, consider implementing secure WebSockets (WSS)
   - This requires additional setup with SSL certificates