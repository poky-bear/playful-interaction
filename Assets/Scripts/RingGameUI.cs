using UnityEngine;
using UnityEngine.UI;

public class RingGameUI : MonoBehaviour
{
    [Header("UI References")]
    public Text instructionsText;
    public Text statusText;
    
    [Header("Game References")]
    public RingGameController gameController;
    
    private int completedRings = 0;
    
    void Start()
    {
        // Find game controller if not assigned
        if (gameController == null)
        {
            gameController = FindObjectOfType<RingGameController>();
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
            instructionsText.text = "Instructions:\n" +
                "1. Press SPACE to expand the dark circle\n" +
                "2. Release SPACE when the circle reaches the bright ring\n" +
                "3. Complete all three rings in the random order\n" +
                "4. Press R to reset the game";
        }
        
        if (statusText != null)
        {
            if (gameController != null && gameController.GameCompleted)
            {
                statusText.text = "Congratulations! You've completed all rings!\nPress R to play again.";
            }
            else
            {
                statusText.text = "Rings completed: " + completedRings + " / 3";
            }
        }
    }
}