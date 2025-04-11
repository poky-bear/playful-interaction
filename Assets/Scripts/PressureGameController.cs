using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PressureGameController : MonoBehaviour
{
    // Reference to the ESP32 Bluetooth Manager
    [SerializeField] public ESP32BluetoothManager bluetoothManager;
    
    // Ring game UI elements
    [SerializeField] public Image outerRing;
    [SerializeField] public Image innerRing;
    [SerializeField] public Image targetZone;
    
    // Ring game parameters
    [SerializeField] private float ringSpeed = 1.0f;
    [SerializeField] private float minRingScale = 0.2f;
    [SerializeField] private float maxRingScale = 0.9f;
    [SerializeField] private float targetZoneSize = 0.1f;
    [SerializeField] private float targetZonePosition = 0.6f;
    
    // Pressure threshold to consider as "pressed" (like space bar)
    [SerializeField] private float pressureThreshold = 30f;
    
    // Score UI
    [SerializeField] public Text scoreText;
    [SerializeField] public Text feedbackText;
    
    // Current pressure value
    private float currentPressure = 0f;
    
    // Ring game state
    private bool isRingExpanding = true;
    private float currentRingScale;
    private int score = 0;
    private bool isGameActive = true;
    
    // Pressure state
    private bool isPressureApplied = false;
    private bool wasPressureApplied = false;

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
        
        // Initialize ring game
        InitializeRingGame();
        
        Debug.Log("PressureGameController initialized. Ready to receive pressure data.");
    }
    
    void InitializeRingGame()
    {
        // Set up initial ring scale
        currentRingScale = minRingScale;
        UpdateRingScale();
        
        // Set up target zone
        if (targetZone != null)
        {
            RectTransform targetRect = targetZone.rectTransform;
            // Set the target zone size and position
            targetRect.localScale = new Vector3(targetZonePosition + targetZoneSize, 1, 1);
            
            // Position the target zone
            Color targetColor = targetZone.color;
            targetColor.a = 0.5f; // Semi-transparent
            targetZone.color = targetColor;
        }
        
        // Initialize score
        UpdateScoreText();
        
        if (feedbackText != null)
        {
            feedbackText.text = "Apply pressure to stop the ring!";
        }
    }
    
    void Update()
    {
        if (!isGameActive) return;
        
        // Check for keyboard input (for testing without pressure sensor)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPressureApplied = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isPressureApplied = false;
        }
        
        // Handle ring expansion/contraction
        if (!isPressureApplied)
        {
            // Ring is moving when no pressure is applied
            if (isRingExpanding)
            {
                currentRingScale += ringSpeed * Time.deltaTime;
                if (currentRingScale >= maxRingScale)
                {
                    currentRingScale = maxRingScale;
                    isRingExpanding = false;
                }
            }
            else
            {
                currentRingScale -= ringSpeed * Time.deltaTime;
                if (currentRingScale <= minRingScale)
                {
                    currentRingScale = minRingScale;
                    isRingExpanding = true;
                }
            }
            
            UpdateRingScale();
        }
        else if (isPressureApplied && !wasPressureApplied)
        {
            // Pressure was just applied - check if ring is in target zone
            CheckRingPosition();
        }
        
        // Update previous pressure state
        wasPressureApplied = isPressureApplied;
    }

    void OnPressureDataReceived(float pressureValue)
    {
        currentPressure = pressureValue;
        
        // Log the pressure value to the console
        Debug.Log("Game received pressure: " + currentPressure);
        
        // Determine if pressure is above threshold (like pressing space)
        isPressureApplied = currentPressure >= pressureThreshold;
    }
    
    void UpdateRingScale()
    {
        if (innerRing != null)
        {
            innerRing.transform.localScale = new Vector3(currentRingScale, currentRingScale, 1);
        }
    }
    
    void CheckRingPosition()
    {
        // Calculate if the ring is within the target zone
        bool isInTargetZone = Mathf.Abs(currentRingScale - targetZonePosition) <= targetZoneSize / 2;
        
        if (isInTargetZone)
        {
            // Success! Ring stopped in target zone
            score += 10;
            ShowFeedback("Perfect! +10 points", Color.green);
        }
        else
        {
            // Missed the target zone
            float distance = Mathf.Abs(currentRingScale - targetZonePosition);
            if (distance <= targetZoneSize)
            {
                // Close but not perfect
                score += 5;
                ShowFeedback("Close! +5 points", Color.yellow);
            }
            else
            {
                // Far from target
                ShowFeedback("Missed!", Color.red);
            }
        }
        
        UpdateScoreText();
        
        // Restart the game after a short delay
        StartCoroutine(RestartRingAfterDelay(1.0f));
    }
    
    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }
    
    void ShowFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        
        Debug.Log(message);
    }
    
    IEnumerator RestartRingAfterDelay(float delay)
    {
        isGameActive = false;
        
        yield return new WaitForSeconds(delay);
        
        // Reset ring position
        currentRingScale = minRingScale;
        isRingExpanding = true;
        UpdateRingScale();
        
        // Reset feedback
        if (feedbackText != null)
        {
            feedbackText.text = "Apply pressure to stop the ring!";
            feedbackText.color = Color.white;
        }
        
        isGameActive = true;
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