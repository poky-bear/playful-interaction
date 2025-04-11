#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

// Bluetooth service and characteristic UUIDs
// These should match the UUIDs in the Unity script
#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define PRESSURE_CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"

// Analog pin for pressure sensor
#define PRESSURE_SENSOR_PIN 36

// BLE Server components
BLEServer* pServer = NULL;
BLECharacteristic* pPressureCharacteristic = NULL;
bool deviceConnected = false;
bool oldDeviceConnected = false;

// Pressure sensor variables
int pressureValue = 0;
int lastPressureValue = 0;
unsigned long lastReadTime = 0;
const int readInterval = 100; // Read every 100ms

// Server callbacks
class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      deviceConnected = true;
      Serial.println("Device connected");
    };

    void onDisconnect(BLEServer* pServer) {
      deviceConnected = false;
      Serial.println("Device disconnected");
    }
};

void setup() {
  Serial.begin(115200);
  Serial.println("ESP32C3 Pressure Sensor BLE Server");

  // Initialize BLE
  BLEDevice::init("ESP32C3_Pressure");
  
  // Create the BLE Server
  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Create the BLE Characteristic for pressure data
  pPressureCharacteristic = pService->createCharacteristic(
                      PRESSURE_CHARACTERISTIC_UUID,
                      BLECharacteristic::PROPERTY_READ   |
                      BLECharacteristic::PROPERTY_WRITE  |
                      BLECharacteristic::PROPERTY_NOTIFY |
                      BLECharacteristic::PROPERTY_INDICATE
                    );

  // Create a BLE Descriptor
  pPressureCharacteristic->addDescriptor(new BLE2902());

  // Start the service
  pService->start();

  // Start advertising
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();
  
  Serial.println("BLE server started, waiting for connections...");
}

void loop() {
  // Current time
  unsigned long currentTime = millis();

  // Read pressure sensor at regular intervals
  if (currentTime - lastReadTime >= readInterval) {
    lastReadTime = currentTime;
    
    // Read the pressure sensor value
    pressureValue = analogRead(PRESSURE_SENSOR_PIN);
    
    // Map the analog reading (which goes from 0 - 4095) to a range of 0 - 100
    pressureValue = map(pressureValue, 0, 4095, 0, 100);
    
    // Print the value to serial monitor
    Serial.print("Pressure value: ");
    Serial.println(pressureValue);
    
    // If the value has changed, update the characteristic
    if (pressureValue != lastPressureValue) {
      lastPressureValue = pressureValue;
      
      // If a device is connected, send the updated value
      if (deviceConnected) {
        // Convert the pressure value to a string
        char pressureStr[8];
        sprintf(pressureStr, "%d", pressureValue);
        
        // Set the characteristic value
        pPressureCharacteristic->setValue(pressureStr);
        pPressureCharacteristic->notify();
        
        Serial.println("Notification sent");
      }
    }
  }

  // Disconnecting
  if (!deviceConnected && oldDeviceConnected) {
    delay(500); // Give the bluetooth stack time to get ready
    pServer->startAdvertising(); // Restart advertising
    Serial.println("Started advertising again");
    oldDeviceConnected = deviceConnected;
  }
  
  // Connecting
  if (deviceConnected && !oldDeviceConnected) {
    oldDeviceConnected = deviceConnected;
  }
}