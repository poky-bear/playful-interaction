#include <WiFi.h>
#include <ESPAsyncWebServer.h>
#include <AsyncTCP.h>
#include <ArduinoJson.h>

// WiFi credentials - replace with your network details
const char* ssid = "YourWiFiName";
const char* password = "YourWiFiPassword";

// Create AsyncWebServer object on port 80
AsyncWebServer server(80);
AsyncWebSocket ws("/ws");

// Pressure sensor pin
const int pressureSensorPin = A0;
int pressureValue = 0;

// LED pin for status indication
const int ledPin = 2;

// Timer for sending data
unsigned long lastSendTime = 0;
const long sendInterval = 100; // Send data every 100ms (10Hz)

void setup() {
  // Initialize serial communication
  Serial.begin(115200);
  
  // Initialize LED pin
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW);
  
  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  
  // Wait for connection with blinking LED
  while (WiFi.status() != WL_CONNECTED) {
    digitalWrite(ledPin, !digitalRead(ledPin)); // Toggle LED
    delay(500);
    Serial.print(".");
  }
  
  // Connected - LED on steady
  digitalWrite(ledPin, HIGH);
  
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.println("IP address: ");
  Serial.println(WiFi.localIP());
  
  // Initialize WebSocket
  ws.onEvent(onWebSocketEvent);
  server.addHandler(&ws);
  
  // Route for root / web page
  server.on("/", HTTP_GET, [](AsyncWebServerRequest *request){
    String html = "<html><body>";
    html += "<h1>ESP32C3 Pressure Sensor</h1>";
    html += "<p>Current pressure: <span id='pressure'>--</span></p>";
    html += "<script>";
    html += "var socket = new WebSocket('ws://' + window.location.hostname + '/ws');";
    html += "socket.onmessage = function(event) {";
    html += "  var data = JSON.parse(event.data);";
    html += "  document.getElementById('pressure').innerText = data.pressure;";
    html += "};";
    html += "</script>";
    html += "</body></html>";
    request->send(200, "text/html", html);
  });
  
  // Start server
  server.begin();
  Serial.println("WebSocket server started");
}

void loop() {
  // Read pressure sensor
  pressureValue = analogRead(pressureSensorPin);
  
  // Map the analog value to a 0-100 range (adjust based on your sensor)
  // For most pressure sensors, you'll need to calibrate this mapping
  int mappedPressure = map(pressureValue, 0, 4095, 0, 100);
  
  // Send pressure data via WebSocket at regular intervals
  unsigned long currentMillis = millis();
  if (currentMillis - lastSendTime >= sendInterval) {
    lastSendTime = currentMillis;
    sendPressureData(mappedPressure);
    
    // Print to serial for debugging
    Serial.print("Pressure: ");
    Serial.println(mappedPressure);
    
    // Blink LED briefly to indicate data transmission
    digitalWrite(ledPin, LOW);
    delay(5);
    digitalWrite(ledPin, HIGH);
  }
  
  // Clean up WebSocket clients
  ws.cleanupClients();
}

void sendPressureData(int pressure) {
  // Create JSON document
  DynamicJsonDocument doc(1024);
  doc["pressure"] = pressure;
  doc["timestamp"] = millis();
  
  // Serialize JSON to string
  String jsonString;
  serializeJson(doc, jsonString);
  
  // Send to all connected WebSocket clients
  ws.textAll(jsonString);
}

void onWebSocketEvent(AsyncWebSocket *server, AsyncWebSocketClient *client, 
                      AwsEventType type, void *arg, uint8_t *data, size_t len) {
  switch (type) {
    case WS_EVT_CONNECT:
      Serial.printf("WebSocket client #%u connected from %s\n", client->id(), client->remoteIP().toString().c_str());
      break;
    case WS_EVT_DISCONNECT:
      Serial.printf("WebSocket client #%u disconnected\n", client->id());
      break;
    case WS_EVT_DATA:
      // Handle incoming data if needed
      break;
    case WS_EVT_PONG:
    case WS_EVT_ERROR:
      break;
  }
}