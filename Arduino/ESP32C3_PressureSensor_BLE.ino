#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLEUtils.h>
#include <BLE2902.h>

// Define the pressure sensor pin
#define PRESSURE_SENSOR_PIN 1  // Change this to the actual pin you're using

// BLE server name
#define DEVICE_NAME "ESP32C3_Pressure"

// UUIDs for the service and characteristic
#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"

// BLE Server, Service, and Characteristic
BLEServer* pServer = NULL;
BLEService* pService = NULL;
BLECharacteristic* pCharacteristic = NULL;

// Flag to track if a device is connected
bool deviceConnected = false;

// Variables for pressure sensor
int rawPressureValue = 0;
float pressureValue = 0.0;

// Calibration values (adjust these based on your sensor)
const int minRawValue = 0;    // Minimum raw value from sensor
const int maxRawValue = 4095; // Maximum raw value from sensor (12-bit ADC)
const float minPressure = 0.0; // Minimum pressure in your desired units
const float maxPressure = 100.0; // Maximum pressure in your desired units

// Smoothing variables
const int numReadings = 10;
int readings[numReadings];
int readIndex = 0;
int total = 0;
int average = 0;

// Server callbacks
class MyServerCallbacks: public BLEServerCallbacks {
  void onConnect(BLEServer* pServer) {
    deviceConnected = true;
    Serial.println("Device connected");
  }

  void onDisconnect(BLEServer* pServer) {
    deviceConnected = false;
    Serial.println("Device disconnected");
    
    // Restart advertising when disconnected
    pServer->startAdvertising();
    Serial.println("Restarting advertising");
  }
};

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  Serial.println("Starting ESP32C3 Pressure Sensor BLE Server...");

  // Initialize the pressure sensor pin
  pinMode(PRESSURE_SENSOR_PIN, INPUT);

  // Initialize smoothing array
  for (int i = 0; i < numReadings; i++) {
    readings[i] = 0;
  }

  // Initialize BLE
  BLEDevice::init(DEVICE_NAME);
  
  // Create the BLE Server
  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Create the BLE Service
  pService = pServer->createService(SERVICE_UUID);

  // Create the BLE Characteristic
  pCharacteristic = pService->createCharacteristic(
                      CHARACTERISTIC_UUID,
                      BLECharacteristic::PROPERTY_READ   |
                      BLECharacteristic::PROPERTY_WRITE  |
                      BLECharacteristic::PROPERTY_NOTIFY |
                      BLECharacteristic::PROPERTY_INDICATE
                    );

  // Create a BLE Descriptor
  pCharacteristic->addDescriptor(new BLE2902());

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
  // Read the pressure sensor
  readPressureSensor();
  
  // If a device is connected, send the pressure data
  if (deviceConnected) {
    // Convert float to bytes
    uint8_t bytes[4];
    memcpy(bytes, &pressureValue, sizeof(pressureValue));
    
    // Set the characteristic value and notify the client
    pCharacteristic->setValue(bytes, 4);
    pCharacteristic->notify();
    
    Serial.print("Sent pressure value: ");
    Serial.println(pressureValue);
  }
  
  // Small delay to avoid flooding
  delay(100);
}

void readPressureSensor() {
  // Subtract the last reading
  total = total - readings[readIndex];
  
  // Read from the sensor
  readings[readIndex] = analogRead(PRESSURE_SENSOR_PIN);
  
  // Add the reading to the total
  total = total + readings[readIndex];
  
  // Advance to the next position in the array
  readIndex = (readIndex + 1) % numReadings;
  
  // Calculate the average
  average = total / numReadings;
  
  // Map the raw value to a pressure value
  pressureValue = mapFloat(average, minRawValue, maxRawValue, minPressure, maxPressure);
  
  // Constrain the value to the valid range
  pressureValue = constrain(pressureValue, minPressure, maxPressure);
  
  // Print to serial for debugging
  Serial.print("Raw: ");
  Serial.print(average);
  Serial.print(", Pressure: ");
  Serial.println(pressureValue);
}

// Custom map function for float values
float mapFloat(float x, float in_min, float in_max, float out_min, float out_max) {
  return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}