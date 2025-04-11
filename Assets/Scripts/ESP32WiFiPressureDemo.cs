using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ESP32WiFiPressureDemo : MonoBehaviour
{
    [SerializeField] private ESP32WiFiManager wifiManager;
    [SerializeField] private Slider pressureSlider;
    [SerializeField] private TextMeshProUGUI pressureText;
    [SerializeField] private Image pressureIndicator;
    [SerializeField] private Color minPressureColor = Color.green;
    [SerializeField] private Color maxPressureColor = Color.red;
    
    // Pressure threshold for triggering actions
    [SerializeField] private float pressureThreshold = 50f;
    
    // Demo cube that changes color based on pressure
    [SerializeField] private GameObject demoCube;
    private Material cubeMaterial;
    
    // Flag to track if pressure is above threshold
    private bool isPressureAboveThreshold = false;
    
    void Start()
    {
        // If no WiFi manager is assigned, try to find one
        if (wifiManager == null)
        {
            wifiManager = FindObjectOfType<ESP32WiFiManager>();
            if (wifiManager == null)
            {
                // Create a new WiFi manager if none exists
                GameObject wifiObj = new GameObject("ESP32WiFiManager");
                wifiManager = wifiObj.AddComponent<ESP32WiFiManager>();
                Debug.Log("Created new ESP32WiFiManager.");
            }
        }
        
        // Subscribe to pressure data events
        wifiManager.OnPressureDataReceived += OnPressureDataReceived;
        
        // Initialize demo cube material
        if (demoCube != null)
        {
            Renderer renderer = demoCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create a new material instance to avoid modifying the shared material
                cubeMaterial = new Material(renderer.material);
                renderer.material = cubeMaterial;
            }
        }
        
        // Initialize UI elements
        if (pressureSlider != null)
        {
            pressureSlider.minValue = 0f;
            pressureSlider.maxValue = 100f;
            pressureSlider.value = 0f;
        }
        
        if (pressureText != null)
        {
            pressureText.text = "Pressure: 0";
        }
        
        if (pressureIndicator != null)
        {
            pressureIndicator.color = minPressureColor;
        }
        
        Debug.Log("ESP32WiFiPressureDemo initialized. Ready to receive pressure data.");
    }
    
    void OnPressureDataReceived(float pressure)
    {
        // Update UI elements
        if (pressureSlider != null)
        {
            pressureSlider.value = pressure;
        }
        
        if (pressureText != null)
        {
            pressureText.text = $"Pressure: {pressure:F1}";
        }
        
        if (pressureIndicator != null)
        {
            // Interpolate color based on pressure
            pressureIndicator.color = Color.Lerp(minPressureColor, maxPressureColor, pressure / 100f);
        }
        
        // Update cube color
        if (cubeMaterial != null)
        {
            cubeMaterial.color = Color.Lerp(minPressureColor, maxPressureColor, pressure / 100f);
        }
        
        // Check if pressure crossed the threshold
        bool isAboveThreshold = pressure >= pressureThreshold;
        
        // Detect threshold crossing events
        if (isAboveThreshold && !isPressureAboveThreshold)
        {
            // Pressure just went above threshold
            OnPressureExceededThreshold();
        }
        else if (!isAboveThreshold && isPressureAboveThreshold)
        {
            // Pressure just went below threshold
            OnPressureBelowThreshold();
        }
        
        // Update threshold state
        isPressureAboveThreshold = isAboveThreshold;
    }
    
    void OnPressureExceededThreshold()
    {
        Debug.Log($"Pressure exceeded threshold ({pressureThreshold})!");
        
        // Make the cube jump or perform some action
        if (demoCube != null)
        {
            Rigidbody rb = demoCube.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
            }
        }
    }
    
    void OnPressureBelowThreshold()
    {
        Debug.Log($"Pressure went below threshold ({pressureThreshold})");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from pressure data events
        if (wifiManager != null)
        {
            wifiManager.OnPressureDataReceived -= OnPressureDataReceived;
        }
    }
}