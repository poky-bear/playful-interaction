using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BluetoothPressureGameController : MonoBehaviour
{
    // Reference to the ESP32 Bluetooth Manager
    [SerializeField] private ESP32BluetoothManager bluetoothManager;
    
    // Pressure threshold to trigger actions (0-100)
    [SerializeField] private float pressureThreshold = 50f;
    
    // Current pressure value
    private float currentPressure = 0f;
    
    // Flag to track if pressure is above threshold
    private bool isPressureActive = false;
    
    // Optional UI elements
    [SerializeField] private Text pressureText;
    [SerializeField] private Slider pressureSlider;
    [SerializeField] private Image pressureIndicator;
    
    // Game state
    private bool gameRunning = false;
    
    void Start()
    {
        // If no Bluetooth manager is assigned, try to find one
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
        
        // Initialize UI elements
        InitializeUI();
        
        // Start the game
        StartGame();
    }
    
    void InitializeUI()
    {
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
            pressureIndicator.color = Color.green;
        }
    }
    
    void StartGame()
    {
        gameRunning = true;
        Debug.Log("Game started. Use pressure to control the game!");
    }
    
    void OnPressureDataReceived(float pressureValue)
    {
        // Update the current pressure value
        currentPressure = pressureValue;
        
        // Update UI
        UpdateUI();
        
        // Check if pressure is above threshold
        bool wasActive = isPressureActive;
        isPressureActive = currentPressure >= pressureThreshold;
        
        // If the pressure state changed, log it
        if (wasActive != isPressureActive)
        {
            if (isPressureActive)
            {
                OnPressureActivated();
            }
            else
            {
                OnPressureDeactivated();
            }
        }
        
        // Log the pressure value to the console
        Debug.Log("Pressure: " + currentPressure + " (Active: " + isPressureActive + ")");
    }
    
    void UpdateUI()
    {
        if (pressureSlider != null)
        {
            pressureSlider.value = currentPressure;
        }
        
        if (pressureText != null)
        {
            pressureText.text = "Pressure: " + currentPressure.ToString("F1");
        }
        
        if (pressureIndicator != null)
        {
            // Change color based on pressure level
            if (isPressureActive)
            {
                pressureIndicator.color = Color.red;
            }
            else
            {
                // Gradient from green to yellow based on pressure
                float t = currentPressure / pressureThreshold;
                pressureIndicator.color = Color.Lerp(Color.green, Color.yellow, t);
            }
        }
    }
    
    void OnPressureActivated()
    {
        Debug.Log("Pressure activated! Value: " + currentPressure);
        
        // This is where you would trigger game actions
        // For example, this could be equivalent to pressing the space bar
        
        // Simulate a space bar press
        SimulateKeyPress(KeyCode.Space, true);
    }
    
    void OnPressureDeactivated()
    {
        Debug.Log("Pressure deactivated! Value: " + currentPressure);
        
        // This is where you would trigger game actions
        // For example, this could be equivalent to releasing the space bar
        
        // Simulate a space bar release
        SimulateKeyPress(KeyCode.Space, false);
    }
    
    void SimulateKeyPress(KeyCode key, bool isDown)
    {
        // This is a placeholder for simulating key presses
        // In a real implementation, you might use Input.GetKeyDown/GetKeyUp in your game logic
        // or use the new Input System's actions
        
        if (isDown)
        {
            Debug.Log("Key down: " + key);
            // Your game logic for key down
        }
        else
        {
            Debug.Log("Key up: " + key);
            // Your game logic for key up
        }
    }
    
    // This method can be called by other game objects to check if pressure is active
    public bool IsPressureActive()
    {
        return isPressureActive;
    }
    
    // This method can be called by other game objects to get the current pressure value
    public float GetCurrentPressure()
    {
        return currentPressure;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from pressure data events
        if (bluetoothManager != null)
        {
            bluetoothManager.OnPressureDataReceived -= OnPressureDataReceived;
        }
        
        // End the game
        gameRunning = false;
    }
}