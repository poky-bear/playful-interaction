using UnityEngine;
using UnityEngine.UI;

public class Player2RingGameUI : MonoBehaviour
{
    [Header("UI References")]
    public Text instructionsText;
    public Text statusText;
    public Text feedbackText; // For showing hit feedback
    
    [Header("Game References")]
    public Player2RingGameController gameController;
    
    private int completedRings = 0;
    
    void Start()
    {
        // Find game controller if not assigned
        if (gameController == null)
        {
            gameController = GetComponent<Player2RingGameController>();
            if (gameController == null)
            {
                gameController = FindObjectOfType<Player2RingGameController>();
            }
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        // Check if the game state has changed
        if (gameController != null && completedRings != gameController.CompletedRings)
        {
            completedRings = gameController.CompletedRings;
            UpdateUI();
        }
        
        // Reset game with R key
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (gameController != null)
            {
                gameController.ResetGame();
                completedRings = 0;
                UpdateUI();
            }
        }
    }
    
    void UpdateUI()
    {
        if (instructionsText != null)
        {
            instructionsText.text = "Player 2 Instructions:\n" +
                "1. Use IJKL to move\n" +
                "2. Press F to expand the dark circle\n" +
                "3. Release F when the circle reaches the bright ring\n" +
                "4. Complete all three rings in the random order\n" +
                "5. Press R to reset the game";
        }
        
        if (statusText != null)
        {
            if (gameController != null && gameController.GameCompleted)
            {
                statusText.text = "Congratulations Player 2! You've completed all rings!\nPress R to play again.";
            }
            else
            {
                statusText.text = "Player 2 Rings completed: " + completedRings + " / 3";
            }
        }
    }
    
    // Called by the game controller when the game is completed
    public void ShowGameCompleteMessage()
    {
        if (statusText != null)
        {
            // Use the success message from the game controller if available
            string message = "Congratulations Player 2! You've completed all rings!\nPress R to play again.";
            if (gameController != null && !string.IsNullOrEmpty(gameController.successMessage))
            {
                message = gameController.successMessage + "\nPress R to play again.";
            }
            
            statusText.text = message;
            statusText.color = Color.green;
            
            // Optional: animate the text
            StartCoroutine(PulseText());
        }
    }
    
    // Coroutine to make the text pulse
    private System.Collections.IEnumerator PulseText()
    {
        float time = 0;
        float duration = 2.0f;
        Color originalColor = statusText.color;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float pulse = 0.7f + 0.3f * Mathf.Sin(t * Mathf.PI * 4);
            
            statusText.transform.localScale = Vector3.one * pulse;
            
            yield return null;
        }
        
        statusText.transform.localScale = Vector3.one;
    }
    
    // Show feedback about the hit accuracy
    public void ShowHitFeedback(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            
            // Make the feedback text fade out
            StartCoroutine(FadeFeedbackText());
        }
    }
    
    // Coroutine to fade out the feedback text
    private System.Collections.IEnumerator FadeFeedbackText()
    {
        if (feedbackText == null)
            yield break;
            
        // Make sure the text is visible
        feedbackText.gameObject.SetActive(true);
        
        // Store original color
        Color originalColor = feedbackText.color;
        
        // Fade out over time
        float duration = 1.5f;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            
            feedbackText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            // Also make the text move up slightly
            feedbackText.transform.position += Vector3.up * Time.deltaTime * 20f;
            
            yield return null;
        }
        
        // Reset position for next time
        feedbackText.transform.localPosition = Vector3.zero;
        feedbackText.gameObject.SetActive(false);
    }
}