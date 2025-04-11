# ESP32C3 Pressure Sensor Bluetooth Integration

This guide explains how to integrate an ESP32C3 microcontroller with a pressure sensor into a Unity game using Bluetooth Low Energy (BLE).

## Hardware Requirements

1. ESP32C3 microcontroller
2. Pressure sensor (analog or digital)
3. Breadboard and jumper wires
4. USB cable for programming the ESP32C3

## Software Requirements

1. Arduino IDE
2. Unity (2019.4 or newer)
3. A Bluetooth plugin for Unity (e.g., [ArduinoBluetoothPlugin](https://github.com/shatalmic/UnityBluetooth) or [Android Bluetooth Low Energy Plugin](https://assetstore.unity.com/packages/tools/network/android-bluetooth-low-energy-185564))

## Step 1: Set Up the ESP32C3 with Arduino IDE

1. Install the ESP32 board package in Arduino IDE:
   - Open Arduino IDE
   - Go to File > Preferences
   - Add `https://raw.githubusercontent.com/espressif/arduino-esp32/gh-pages/package_esp32_index.json` to the "Additional Boards Manager URLs" field
   - Go to Tools > Board > Boards Manager
   - Search for "esp32" and install the ESP32 package

2. Select the correct board:
   - Go to Tools > Board > ESP32 Arduino > ESP32C3 Dev Module
   - Set the appropriate port under Tools > Port

3. Install required libraries:
   - Go to Sketch > Include Library > Manage Libraries
   - Search for and install "ESP32 BLE Arduino"

## Step 2: Wire the Pressure Sensor to ESP32C3

1. Connect the pressure sensor to the ESP32C3:
   - VCC pin of the sensor to 3.3V on ESP32C3
   - GND pin of the sensor to GND on ESP32C3
   - Signal pin of the sensor to an analog input pin (e.g., GPIO1)

2. For digital pressure sensors, follow the manufacturer's wiring diagram.

## Step 3: Upload the Arduino Code

1. Open the `ESP32C3_PressureSensor_BLE.ino` file in Arduino IDE
2. Verify that the pin number in the code matches your wiring:
   ```cpp
   #define PRESSURE_SENSOR_PIN 1  // Change this to the actual pin you're using
   ```
3. Adjust the calibration values based on your sensor:
   ```cpp
   const int minRawValue = 0;    // Minimum raw value from sensor
   const int maxRawValue = 4095; // Maximum raw value from sensor (12-bit ADC)
   const float minPressure = 0.0; // Minimum pressure in your desired units
   const float maxPressure = 100.0; // Maximum pressure in your desired units
   ```
4. Upload the code to your ESP32C3
5. Open the Serial Monitor (Tools > Serial Monitor) and set the baud rate to 115200
6. Verify that the ESP32C3 is advertising as a BLE device and reading pressure values

## Step 4: Set Up Unity Project

1. Import a Bluetooth plugin into your Unity project:
   - For this example, we assume you're using a plugin that provides Bluetooth functionality
   - Follow the plugin's installation instructions

2. Add the provided scripts to your Unity project:
   - `ESP32BluetoothManager.cs`: Handles the Bluetooth connection and data reception
   - `BluetoothPressureGameController.cs`: Uses the pressure data to control game elements

3. Create a new scene or use an existing one
4. Create an empty GameObject and add the `ESP32BluetoothManager` component
5. Create another GameObject for your game controller and add the `BluetoothPressureGameController` component
6. Assign the ESP32BluetoothManager to the BluetoothPressureGameController's `bluetoothManager` field

## Step 5: Configure the Game Controller

1. Set the pressure threshold in the BluetoothPressureGameController component:
   - This determines at what pressure level the controller will trigger actions
   - Default is 50 (on a scale of 0-100)

2. Optionally, add UI elements to visualize the pressure:
   - Create a Text element for displaying the pressure value
   - Create a Slider for visualizing the pressure level
   - Create an Image that changes color based on pressure
   - Assign these UI elements to the corresponding fields in the BluetoothPressureGameController

## Step 6: Implement Game Logic

1. Use the BluetoothPressureGameController in your game:
   ```csharp
   // Get a reference to the controller
   BluetoothPressureGameController pressureController;
   
   void Start() {
       pressureController = FindObjectOfType<BluetoothPressureGameController>();
   }
   
   void Update() {
       // Check if pressure is active
       if (pressureController.IsPressureActive()) {
           // Do something when pressure is above threshold
       }
       
       // Get the current pressure value
       float pressure = pressureController.GetCurrentPressure();
       // Use the pressure value for continuous control
   }
   ```

2. Alternatively, modify the BluetoothPressureGameController to use Unity's event system to broadcast pressure changes

## Step 7: Test the Integration

1. Build and run your Unity game on a device with Bluetooth support
2. Make sure the ESP32C3 is powered on and running the BLE server code
3. The game should automatically connect to the ESP32C3 and start receiving pressure data
4. Apply pressure to the sensor and verify that it triggers the expected actions in the game

## Troubleshooting

1. **No connection**: 
   - Ensure the ESP32C3 is powered and running the correct code
   - Check that Bluetooth is enabled on your device
   - Verify that the device name and UUIDs match between the Arduino code and Unity script

2. **Erratic pressure readings**:
   - Check the sensor wiring
   - Adjust the smoothing algorithm in the Arduino code
   - Calibrate the sensor by adjusting the min/max values

3. **Delayed response**:
   - Reduce the delay in the Arduino loop
   - Optimize the Bluetooth connection parameters
   - Consider using a different communication method for lower latency

## Advanced Features

1. **Multiple sensors**: Modify the code to support multiple pressure sensors by adding more characteristics to the BLE service

2. **Two-way communication**: Implement commands from Unity to the ESP32C3 by adding writable characteristics

3. **Data logging**: Add functionality to record pressure data for analysis or replay

## Resources

- [ESP32 BLE Documentation](https://docs.espressif.com/projects/esp-idf/en/latest/esp32/api-reference/bluetooth/index.html)
- [Unity Manual: Plugins](https://docs.unity3d.com/Manual/Plugins.html)
- [Arduino BLE Library Reference](https://github.com/nkolban/ESP32_BLE_Arduino)