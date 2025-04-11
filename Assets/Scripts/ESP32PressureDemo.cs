using UnityEngine;
using UnityEngine.UI;

public class ESP32PressureDemo : MonoBehaviour
{
    // Reference to the ESP32 Bluetooth Manager
    [SerializeField] private ESP32BluetoothManager bluetoothManager;
    
    // UI Text to display pressure value
    [SerializeField] private Text pressureText;
    
    // UI Slider to visualize pressure
    [SerializeField] private Slider pressureSlider;
    
    // GameObject that will be scaled based on pressure
    [SerializeField] private GameObject pressureIndicator;
    
    // Current pressure value
    private float currentPressure = 0f;

    void Start()
    {
        // If no bluetooth manager is assigned, try to find one
        if (bluetoothManager == null)
        {
            bluetoothManager = FindObjectOfType<ESP32BluetoothManager>();
            if (bluetoothManager == null)
            {
                Debug.LogError("No ESP32BluetoothManager found in the scene. Please add one.");
                return;
            }
        }
        
        // Subscribe to pressure data events
        bluetoothManager.OnPressureDataReceived += OnPressureDataReceived;
        
        Debug.Log("ESP32PressureDemo initialized. Ready to receive pressure data.");
        
        // Initialize UI elements
        if (pressureText != null)
        {
            pressureText.text = "Pressure: 0";
        }
        
        if (pressureSlider != null)
        {
            pressureSlider.minValue = 0f;
            pressureSlider.maxValue = 100f;
            pressureSlider.value = 0f;
        }
    }

    void OnPressureDataReceived(float pressureValue)
    {
        currentPressure = pressureValue;
        
        // Update UI
        if (pressureText != null)
        {
            pressureText.text = "Pressure: " + currentPressure.ToString("F1");
        }
        
        if (pressureSlider != null)
        {
            pressureSlider.value = currentPressure;
        }
        
        // Update pressure indicator
        if (pressureIndicator != null)
        {
            // Scale the indicator based on pressure
            float scale = 1f + (currentPressure / 100f);
            pressureIndicator.transform.localScale = new Vector3(scale, scale, scale);
        }
        
        // Log the pressure value to the console
        Debug.Log("Demo received pressure: " + currentPressure);
    }

    void OnDestroy()
    {
        // Unsubscribe from pressure data events
        if (bluetoothManager != null)
        {
            bluetoothManager.OnPressureDataReceived -= OnPressureDataReceived;
        }
    }
}