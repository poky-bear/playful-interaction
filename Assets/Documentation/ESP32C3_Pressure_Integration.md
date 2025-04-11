# ESP32C3 Pressure Sensor Integration Guide

This guide explains how to integrate an ESP32C3 microcontroller with a pressure sensor into your Unity game using Bluetooth Low Energy (BLE).

## Hardware Requirements

1. ESP32C3 microcontroller
2. Pressure sensor (analog)
3. Breadboard and jumper wires
4. USB cable for programming the ESP32C3

## Software Requirements

1. Arduino IDE
2. Unity (with this project)
3. Required Arduino libraries:
   - BLEDevice
   - BLEServer
   - BLEUtils
   - BLE2902

## Step 1: Set Up the Arduino Environment

1. Install the Arduino IDE from [arduino.cc](https://www.arduino.cc/en/software)
2. Add ESP32 board support to Arduino IDE:
   - Open Arduino IDE
   - Go to File > Preferences
   - Add `https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json` to the "Additional Boards Manager URLs" field
   - Go to Tools > Board > Boards Manager
   - Search for "esp32" and install the ESP32 package

## Step 2: Wire Up the Pressure Sensor

1. Connect the pressure sensor to the ESP32C3:
   - Connect VCC of the sensor to 3.3V on the ESP32C3
   - Connect GND of the sensor to GND on the ESP32C3
   - Connect the analog output of the sensor to pin 36 (or another analog pin, update the code accordingly)

## Step 3: Upload the Arduino Code

1. Open the Arduino IDE
2. Open the `ESP32C3_PressureSensor.ino` file provided in this project
3. Select the correct board from Tools > Board > ESP32 Arduino > ESP32C3 Dev Module
4. Select the correct port from Tools > Port
5. Click the Upload button
6. Open the Serial Monitor (Tools > Serial Monitor) and set the baud rate to 115200 to verify the ESP32C3 is running correctly

## Step 4: Set Up the Unity Project

1. Open the Unity project
2. Create a new scene or use an existing one
3. Create an empty GameObject and name it "BluetoothManager"
4. Add the `ESP32BluetoothManager.cs` script to this GameObject
5. Create or select a GameObject that you want to control with the pressure sensor
6. Create another empty GameObject and name it "GameController"
7. Add the `PressureGameController.cs` script to this GameObject
8. In the Inspector for the GameController:
   - Assign the BluetoothManager to the "Bluetooth Manager" field
   - Assign the GameObject you want to control to the "Controlled Object" field
   - Adjust the sensitivity and height parameters as needed

## Step 5: Run the Game

1. Make sure your ESP32C3 is powered on and running the pressure sensor code
2. Press Play in the Unity Editor
3. The Unity game should automatically connect to the ESP32C3 via Bluetooth
4. You should see debug messages in the console indicating the connection status and pressure values
5. The controlled GameObject should respond to pressure changes

## Troubleshooting

### Bluetooth Connection Issues

1. Make sure Bluetooth is enabled on your computer
2. Verify the ESP32C3 is powered and running (check the Serial Monitor)
3. Check that the UUIDs in the Arduino code match those in the Unity script
4. Restart both the ESP32C3 and the Unity game

### Pressure Sensor Issues

1. Check the wiring connections
2. Verify the sensor is working by monitoring the Serial output from the ESP32C3
3. Adjust the mapping range in the Arduino code if necessary

### Unity Integration Issues

1. Check the Unity console for error messages
2. Verify all script references are properly assigned in the Inspector
3. Make sure the Bluetooth permissions are granted in your application

## Notes on Real Implementation

The Unity scripts provided include simulated Bluetooth functionality. For a real implementation, you would need to use a Bluetooth plugin for Unity, such as:

- [Android Bluetooth Plugin](https://assetstore.unity.com/packages/tools/network/android-ios-bluetooth-le-plugin-68483)
- [Unity Bluetooth LE](https://github.com/adrenak/UniLE)
- [ArduinoBluetoothAPI](https://assetstore.unity.com/packages/tools/input-management/arduino-bluetooth-plugin-98960)

These plugins provide the actual Bluetooth functionality that would replace the simulated methods in the provided scripts.

## Customization

- Adjust the `pressureSensitivity` parameter in the PressureGameController to change how responsive the game is to pressure changes
- Modify the `ProcessPressureData` method in ESP32BluetoothManager to implement different behaviors based on pressure values
- Change the mapping range in the Arduino code to match your specific pressure sensor's characteristics