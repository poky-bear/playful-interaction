using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

// This is a simplified WebSocket implementation that doesn't require external libraries
// For production, you would use a proper WebSocket library like NativeWebSocket
public class ESP32WiFiManager : MonoBehaviour
{
    // WebSocket URL - replace with your ESP32's IP address
    [SerializeField] private string webSocketUrl = "ws://192.168.1.100/ws";
    
    // Simulated WebSocket for testing without external dependencies
    private SimulatedWebSocket webSocket;
    
    // Latest pressure value received
    private float pressureValue = 0f;
    
    // Event to notify when pressure data is received
    public event Action<float> OnPressureDataReceived;
    
    // Connection status
    private bool isConnected = false;
    
    // For testing in the Unity Editor
    [SerializeField] private bool useSimulatedData = true;
    [SerializeField] private float simulatedMinPressure = 0f;
    [SerializeField] private float simulatedMaxPressure = 100f;
    [SerializeField] private float simulationUpdateInterval = 0.1f;
    
    void Start()
    {
        // Initialize WebSocket
        webSocket = new SimulatedWebSocket(webSocketUrl);
        
        // Register WebSocket events
        webSocket.OnOpen += OnWebSocketOpen;
        webSocket.OnError += OnWebSocketError;
        webSocket.OnClose += OnWebSocketClose;
        webSocket.OnMessage += OnWebSocketMessage;
        
        // Connect to the WebSocket server
        Debug.Log("Connecting to WebSocket server...");
        ConnectToWebSocket();
        
        // Start simulated data if enabled
        if (useSimulatedData)
        {
            StartCoroutine(SimulatePressureData());
        }
    }
    
    async void ConnectToWebSocket()
    {
        try
        {
            await webSocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket connection error: {e.Message}");
            StartCoroutine(ReconnectAfterDelay(3.0f));
        }
    }
    
    void Update()
    {
        // Process WebSocket messages
        if (webSocket != null)
        {
            webSocket.DispatchMessageQueue();
        }
    }
    
    void OnWebSocketOpen()
    {
        Debug.Log("Connected to WebSocket server");
        isConnected = true;
    }
    
    void OnWebSocketError(string errorMsg)
    {
        Debug.LogError("WebSocket error: " + errorMsg);
    }
    
    void OnWebSocketClose(int code)
    {
        Debug.Log("WebSocket connection closed with code: " + code);
        isConnected = false;
        
        // Try to reconnect
        StartCoroutine(ReconnectAfterDelay(3.0f));
    }
    
    void OnWebSocketMessage(byte[] data)
    {
        // Convert byte array to string
        string jsonMessage = Encoding.UTF8.GetString(data);
        
        try
        {
            // Parse the JSON message
            PressureData pressureData = JsonUtility.FromJson<PressureData>(jsonMessage);
            
            // Update pressure value
            pressureValue = pressureData.pressure;
            
            // Log the pressure value
            Debug.Log("Received pressure: " + pressureValue);
            
            // Notify listeners
            OnPressureDataReceived?.Invoke(pressureValue);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing WebSocket message: {e.Message}");
        }
    }
    
    IEnumerator ReconnectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!isConnected)
        {
            Debug.Log("Attempting to reconnect...");
            ConnectToWebSocket();
        }
    }
    
    IEnumerator SimulatePressureData()
    {
        while (useSimulatedData)
        {
            // Generate random pressure value
            float randomPressure = UnityEngine.Random.Range(simulatedMinPressure, simulatedMaxPressure);
            
            // Create simulated pressure data
            PressureData data = new PressureData
            {
                pressure = randomPressure,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            // Convert to JSON
            string jsonData = JsonUtility.ToJson(data);
            
            // Simulate receiving message
            OnWebSocketMessage(Encoding.UTF8.GetBytes(jsonData));
            
            // Wait for next update
            yield return new WaitForSeconds(simulationUpdateInterval);
        }
    }
    
    // Class to deserialize JSON messages
    [Serializable]
    private class PressureData
    {
        public float pressure;
        public long timestamp;
    }
    
    async void OnDestroy()
    {
        // Close WebSocket connection when object is destroyed
        if (webSocket != null)
        {
            await webSocket.Close();
            
            // Unregister events
            webSocket.OnOpen -= OnWebSocketOpen;
            webSocket.OnError -= OnWebSocketError;
            webSocket.OnClose -= OnWebSocketClose;
            webSocket.OnMessage -= OnWebSocketMessage;
        }
    }
    
    // Simulated WebSocket class for testing without external dependencies
    private class SimulatedWebSocket
    {
        private string url;
        private bool isConnected = false;
        
        public event Action OnOpen;
        public event Action<string> OnError;
        public event Action<int> OnClose;
        public event Action<byte[]> OnMessage;
        
        public SimulatedWebSocket(string url)
        {
            this.url = url;
        }
        
        public async Task Connect()
        {
            // Simulate connection delay
            await Task.Delay(500);
            
            // Simulate successful connection
            isConnected = true;
            OnOpen?.Invoke();
        }
        
        public void DispatchMessageQueue()
        {
            // This would process incoming messages in a real implementation
            // In our simulation, we don't need to do anything here
        }
        
        public async Task Close()
        {
            if (isConnected)
            {
                // Simulate closing delay
                await Task.Delay(100);
                
                isConnected = false;
                OnClose?.Invoke(1000); // 1000 = normal closure
            }
        }
        
        public void Send(string message)
        {
            if (isConnected)
            {
                Debug.Log($"Simulated WebSocket sending: {message}");
            }
            else
            {
                OnError?.Invoke("Cannot send message: WebSocket is not connected");
            }
        }
    }
}