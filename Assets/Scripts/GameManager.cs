using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display game over message")]
    public Text gameOverText;
    
    [Tooltip("Panel to show when game is over")]
    public GameObject gameOverPanel;
    
    [Header("Game Settings")]
    [Tooltip("Time to wait before allowing restart")]
    public float restartDelay = 1f;
    
    private bool isGameOver = false;
    
    private void Start()
    {
        // Ensure game starts in playing state
        Time.timeScale = 1f;
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Allow restart with R key after game over
        if (isGameOver && Input.GetKeyDown(KeyCode.R) && Time.unscaledTime > restartDelay)
        {
            RestartGame();
        }
    }
    
    public void EndGame(string message = "Game Over!")
    {
        if (isGameOver) return; // Prevent multiple calls
        
        isGameOver = true;
        Time.timeScale = 0f; // Pause the game
        
        // Show game over UI if available
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = message + "\nPress R to restart";
            }
        }
        
        Debug.Log($"[GameManager] {message}");
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}