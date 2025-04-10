using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton instance
    public static PlayerManager Instance { get; private set; }
    
    // List of all players in the game
    [HideInInspector]
    public List<GameObject> players = new List<GameObject>();
    
    // List of players who have completed the game
    [HideInInspector]
    public List<GameObject> completedPlayers = new List<GameObject>();
    
    // The number of boids per player
    public int boidsPerPlayer = 12;
    
    // The maximum number of boids that can follow a player
    public int maxBoidsPerCompletedPlayer = 6;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Register a player with the manager
    public void RegisterPlayer(GameObject player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log("Player registered. Total players: " + players.Count);
        }
    }
    
    // Register a player as having completed the game
    public void RegisterCompletedPlayer(GameObject player)
    {
        if (!completedPlayers.Contains(player))
        {
            completedPlayers.Add(player);
            Debug.Log("Player completed game. Total completed: " + completedPlayers.Count);
        }
    }
    
    // Get the total number of boids that should be in the game
    public int GetTotalBoidCount()
    {
        return players.Count * boidsPerPlayer;
    }
    
    // Check if a player has completed the game
    public bool HasPlayerCompleted(GameObject player)
    {
        return completedPlayers.Contains(player);
    }
}