using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerRingGameUI : MonoBehaviour
{
    [Header("UI References")]
    public Text instructionsText;
    public Text statusText;
    public Text feedbackText;
    
    [Header("UI Settings")]
    public string defaultInstructions = "When players are close, press SPACE (Player 1) and F (Player 2) together to expand the ring!";
    public string activatedInstructions = "Both players press and hold your buttons (SPACE & F) together, then release to match the purple ring! Both must succeed to advance!";
    public string completedMessage = "Congratulations! Both players completed the challenge together!";
    public string syncInstructions = "You must synchronize your timing! Both players need to hit the target to advance.";
    
    [Header("References")]
    public MultiplayerRingGame gameController;
    
    private float messageTimer = 0f;
    private float messageDuration = 2f;
    private bool showingFeedback = false;
    
    void Start()
    {
        // Find the game controller if not assigned
        if (gameController == null)
        {
            gameController = FindObjectOfType<MultiplayerRingGame>();
            if (gameController == null)
            {
                Debug.LogError("MultiplayerRingGame component not found in the scene!");
            }
        }
        
        // Initialize UI
        if (instructionsText != null)
        {
            instructionsText.text = defaultInstructions;
        }
        
        if (statusText != null)
        {
            statusText.text = "Multiplayer mode inactive";
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
    
    void Update()
    {
        if (gameController == null)
            return;
            
        // Update UI based on game state
        if (gameController.GameCompleted)
        {
            if (statusText != null)
            {
                statusText.text = "Multiplayer Challenge Completed!";
            }
            
            if (instructionsText != null)
            {
                instructionsText.text = "Move away from each other to reset.";
            }
        }
        else if (gameController.MultiplayerModeActive)
        {
            if (statusText != null)
            {
                statusText.text = "Multiplayer Mode Active! Rings completed: " + 
                                 gameController.CompletedRings + "/3";
            }
            
            if (instructionsText != null)
            {
                // Alternate between the main instructions and sync instructions
                float time = Time.time % 10f;
                if (time < 5f)
                {
                    instructionsText.text = activatedInstructions;
                }
                else
                {
                    instructionsText.text = syncInstructions;
                }
            }
        }
        else
        {
            // Check if players are getting closer to activation
            float proximityPercentage = Mathf.Clamp01(gameController.GetProximityPercentage());
            
            if (statusText != null)
            {
                if (proximityPercentage > 0)
                {
                    statusText.text = "Players getting closer... " + 
                                     (proximityPercentage * 100).ToString("F0") + "% to activation";
                }
                else
                {
                    statusText.text = "Multiplayer mode inactive";
                }
            }
        }
        
        // Handle temporary feedback messages
        if (showingFeedback)
        {
            messageTimer += Time.deltaTime;
            if (messageTimer >= messageDuration)
            {
                if (feedbackText != null)
                {
                    feedbackText.text = "";
                }
                showingFeedback = false;
            }
        }
    }
    
    // Show feedback message with specified color
    public void ShowHitFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            showingFeedback = true;
            messageTimer = 0f;
        }
    }
    
    // Show game completion message
    public void ShowGameCompleteMessage()
    {
        if (feedbackText != null)
        {
            feedbackText.text = completedMessage;
            feedbackText.color = Color.green;
            showingFeedback = true;
            messageTimer = 0f;
            
            // Make this message stay longer
            messageDuration = 5f;
        }
    }
}