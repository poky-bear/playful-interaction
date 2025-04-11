using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using NativeWebSocket;

public class ESP32WebSocketManager : MonoBehaviour
{
    // ESP32 WebSocket URL
    [SerializeField] private string webSocketUrl = "ws://192.168.1.100/ws";
    
    // Option to use simulated data (for testing without ESP32)
    [SerializeField] private bool useSimulatedData = true;
    
    // WebSocket instance
    private WebSocket websocket;
    
    // Latest pressure value received
    private float pressureValue = 0f;
    
    // Event to notify when pressure data is received
    public event Action<float> OnPressureDataReceived;
    
    // Connection status
    private bool isConnected = false;
    
    // Reconnection parameters
    [SerializeField] private float reconnectDelay = 3.0f;
    [SerializeField] private int maxReconnectAttempts = 5;
    private int reconnectAttempts = 0;
    
    async void Start()
    {
        Debug.Log("ESP32WebSocketManager: Initializing...");
        
        if (useSimulatedData)
        {
            Debug.Log("ESP32WebSocketManager: Using simulated data");
            StartCoroutine(SimulateDataReceiving());
            return;
        }
        
        // Create WebSocket instance
        websocket = new WebSocket(webSocketUrl);
        
        // Register WebSocket events
        websocket.OnOpen += OnWebSocketOpen;
        websocket.OnError += OnWebSocketError;
        websocket.OnClose += OnWebSocketClose;
        websocket.OnMessage += OnWebSocketMessage;
        
        // Connect to the WebSocket server
        Debug.Log("ESP32WebSocketManager: Connecting to WebSocket server at " + webSocketUrl);
        await websocket.Connect();
    }
    
    void Update()
    {
        if (!useSimulatedData && websocket != null)
        {
            // Keep the WebSocket connection alive
            websocket.DispatchMessageQueue();
        }
    }
    
    void OnWebSocketOpen()
    {
        Debug.Log("ESP32WebSocketManager: Connected to WebSocket server");
        isConnected = true;
        reconnectAttempts = 0;
    }
    
    void OnWebSocketError(string errorMsg)
    {
        Debug.LogError("ESP32WebSocketManager: WebSocket error: " + errorMsg);
    }
    
    void OnWebSocketClose(WebSocketCloseCode code)
    {
        Debug.Log("ESP32WebSocketManager: WebSocket connection closed with code: " + code);
        isConnected = false;
        
        // Try to reconnect if not too many attempts
        if (reconnectAttempts < maxReconnectAttempts)
        {
            StartCoroutine(ReconnectAfterDelay());
        }
        else
        {
            Debug.LogWarning("ESP32WebSocketManager: Max reconnect attempts reached. Switching to simulated data.");
            useSimulatedData = true;
            StartCoroutine(SimulateDataReceiving());
        }
    }
    
    void OnWebSocketMessage(byte[] data)
    {
        // Convert byte array to string
        string jsonMessage = System.Text.Encoding.UTF8.GetString(data);
        
        try
        {
            // Parse the JSON message
            PressureData pressureData = JsonUtility.FromJson<PressureData>(jsonMessage);
            
            // Update pressure value
            pressureValue = pressureData.pressure;
            
            // Log the pressure value
            Debug.Log("ESP32WebSocketManager: Received pressure: " + pressureValue);
            
            // Notify listeners
            OnPressureDataReceived?.Invoke(pressureValue);
        }
        catch (Exception e)
        {
            Debug.LogError("ESP32WebSocketManager: Error parsing WebSocket message: " + e.Message);
            Debug.LogError("ESP32WebSocketManager: Message content: " + jsonMessage);
        }
    }
    
    IEnumerator ReconnectAfterDelay()
    {
        reconnectAttempts++;
        float delay = reconnectDelay * reconnectAttempts; // Exponential backoff
        
        Debug.Log($"ESP32WebSocketManager: Attempting to reconnect in {delay} seconds (attempt {reconnectAttempts}/{maxReconnectAttempts})");
        
        yield return new WaitForSeconds(delay);
        
        if (websocket != null && websocket.State == WebSocketState.Closed)
        {
            Debug.Log("ESP32WebSocketManager: Reconnecting to WebSocket server...");
            websocket.Connect();
        }
    }
    
    IEnumerator SimulateDataReceiving()
    {
        Debug.Log("ESP32WebSocketManager: Starting to receive simulated pressure data...");
        
        while (true)
        {
            // Simulate receiving data
            float simulatedPressure = UnityEngine.Random.Range(0f, 100f);
            
            // Update pressure value
            pressureValue = simulatedPressure;
            
            // Log the pressure value
            Debug.Log("ESP32WebSocketManager: Simulated pressure: " + pressureValue);
            
            // Notify listeners
            OnPressureDataReceived?.Invoke(pressureValue);
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    // Class to deserialize JSON messages
    [Serializable]
    private class PressureData
    {
        public float pressure;
        public long timestamp;
    }
    
    public float GetPressureValue()
    {
        return pressureValue;
    }
    
    async void OnDestroy()
    {
        // Clean up WebSocket connection when object is destroyed
        if (!useSimulatedData && websocket != null && websocket.State == WebSocketState.Open)
        {
            Debug.Log("ESP32WebSocketManager: Closing WebSocket connection");
            await websocket.Close();
        }
    }
    
    // Method to test the WebSocket connection with simulated data
    public void TestWebSocketConnection()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            // Create a test message
            string testJson = "{\"pressure\": 50.0, \"timestamp\": 12345}";
            
            // Simulate receiving the message
            ((WebSocket)websocket).SimulateMessage(testJson);
        }
        else
        {
            Debug.LogWarning("ESP32WebSocketManager: Cannot test WebSocket connection - not connected");
        }
    }
}