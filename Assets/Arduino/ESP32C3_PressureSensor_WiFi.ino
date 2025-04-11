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
const int ledPin = 2;  // Most ESP32C3 boards have an onboard LED on pin 2

void setup() {
  Serial.begin(115200);
  
  // Initialize LED pin
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, LOW);
  
  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
  
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
    // Blink LED while connecting
    digitalWrite(ledPin, !digitalRead(ledPin));
  }
  
  // Turn LED on when connected
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
    request->send(200, "text/html", getIndexHTML());
  });
  
  // Start server
  server.begin();
  Serial.println("WebSocket server started");
}

void loop() {
  // Read pressure sensor
  pressureValue = analogRead(pressureSensorPin);
  
  // Map the analog value to a 0-100 range for easier use in Unity
  // Adjust min/max values based on your specific pressure sensor
  int mappedPressure = map(pressureValue, 0, 4095, 0, 100);
  
  // Send pressure data via WebSocket every 100ms
  static unsigned long lastSendTime = 0;
  if (millis() - lastSendTime > 100) {
    sendPressureData(mappedPressure);
    lastSendTime = millis();
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
  
  // Also print to serial for debugging
  static unsigned long lastPrintTime = 0;
  if (millis() - lastPrintTime > 1000) {  // Print once per second
    Serial.print("Pressure: ");
    Serial.println(pressure);
    lastPrintTime = millis();
  }
}

void onWebSocketEvent(AsyncWebSocket *server, AsyncWebSocketClient *client, 
                      AwsEventType type, void *arg, uint8_t *data, size_t len) {
  switch (type) {
    case WS_EVT_CONNECT:
      Serial.printf("WebSocket client #%u connected from %s\n", client->id(), client->remoteIP().toString().c_str());
      // Blink LED twice to indicate client connection
      blinkLED(2, 200);
      break;
    case WS_EVT_DISCONNECT:
      Serial.printf("WebSocket client #%u disconnected\n", client->id());
      // Blink LED once to indicate client disconnection
      blinkLED(1, 500);
      break;
    case WS_EVT_DATA:
      // Handle incoming data if needed
      handleWebSocketMessage(arg, data, len);
      break;
    case WS_EVT_PONG:
    case WS_EVT_ERROR:
      break;
  }
}

void handleWebSocketMessage(void *arg, uint8_t *data, size_t len) {
  AwsFrameInfo *info = (AwsFrameInfo*)arg;
  if (info->final && info->index == 0 && info->len == len && info->opcode == WS_TEXT) {
    data[len] = 0;
    String message = (char*)data;
    Serial.print("Received message: ");
    Serial.println(message);
    
    // Here you could handle commands from Unity if needed
  }
}

void blinkLED(int times, int delayMs) {
  for (int i = 0; i < times; i++) {
    digitalWrite(ledPin, HIGH);
    delay(delayMs);
    digitalWrite(ledPin, LOW);
    delay(delayMs);
  }
  digitalWrite(ledPin, HIGH);  // Turn LED back on
}

String getIndexHTML() {
  String html = "<!DOCTYPE html><html>";
  html += "<head><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">";
  html += "<title>ESP32C3 Pressure Sensor</title>";
  html += "<style>";
  html += "body { font-family: Arial, sans-serif; text-align: center; margin: 20px; }";
  html += ".container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ccc; border-radius: 10px; }";
  html += ".value { font-size: 24px; font-weight: bold; margin: 20px 0; }";
  html += ".meter { width: 100%; height: 30px; border: 1px solid #ccc; border-radius: 5px; margin: 20px 0; }";
  html += ".bar { height: 100%; background-color: #4CAF50; width: 0%; transition: width 0.3s; }";
  html += "</style>";
  html += "</head><body>";
  html += "<div class=\"container\">";
  html += "<h1>ESP32C3 Pressure Sensor</h1>";
  html += "<p>Current pressure:</p>";
  html += "<div class=\"value\" id=\"pressure\">0</div>";
  html += "<div class=\"meter\"><div class=\"bar\" id=\"pressureBar\"></div></div>";
  html += "<p>IP Address: " + WiFi.localIP().toString() + "</p>";
  html += "<p>WebSocket: ws://" + WiFi.localIP().toString() + "/ws</p>";
  html += "</div>";
  
  // JavaScript for WebSocket connection
  html += "<script>";
  html += "var gateway = `ws://${window.location.hostname}/ws`;";
  html += "var websocket;";
  html += "window.addEventListener('load', onLoad);";
  html += "function initWebSocket() {";
  html += "  console.log('Trying to open a WebSocket connection...');";
  html += "  websocket = new WebSocket(gateway);";
  html += "  websocket.onopen = onOpen;";
  html += "  websocket.onclose = onClose;";
  html += "  websocket.onmessage = onMessage;";
  html += "}";
  html += "function onOpen(event) {";
  html += "  console.log('Connection opened');";
  html += "}";
  html += "function onClose(event) {";
  html += "  console.log('Connection closed');";
  html += "  setTimeout(initWebSocket, 2000);";
  html += "}";
  html += "function onMessage(event) {";
  html += "  var data = JSON.parse(event.data);";
  html += "  document.getElementById('pressure').innerHTML = data.pressure;";
  html += "  document.getElementById('pressureBar').style.width = data.pressure + '%';";
  html += "}";
  html += "function onLoad(event) {";
  html += "  initWebSocket();";
  html += "}";
  html += "</script>";
  html += "</body></html>";
  
  return html;
}