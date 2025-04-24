using UnityEngine;

public class PredatorBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed")]
    public float maxSpeed = 2f;  // Slower than boids
    
    [Tooltip("Minimum movement speed")]
    public float minSpeed = 1f;  // Keep some movement
    
    [Tooltip("How strongly the predator steers")]
    public float maxSteerForce = 1f;  // Gentle steering
    
    [Tooltip("How strongly the predator is attracted to players")]
    public float attractionWeight = 0.5f;  // Gentle attraction
    
    [Tooltip("Minimum distance before considering it a 'hit'")]
    public float hitDistance = 0.5f;  // Distance to count as touching
    
    [Tooltip("Distance at which attraction starts")]
    public float attractionRadius = 10f;  // Start being attracted from far
    
    [Header("References")]
    public GameObject player1;
    public GameObject player2;
    
    private Vector3 velocity;  // Current movement velocity
    private int totalHits = 0;
    
    private void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("[Predator] Player references not set! Player1: " + (player1 != null) + ", Player2: " + (player2 != null));
            return;
        }

        Debug.Log($"[Predator] Initialized with Player1: {player1.name}, Player2: {player2.name}");
        
        // Initialize velocity
        float startSpeed = (minSpeed + maxSpeed) / 2f;
        velocity = transform.forward * startSpeed;
    }
    
    private void Update()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("[Predator] One or both players are missing!");
            return;
        }

        Vector3 player1Pos = player1.transform.position;
        Vector3 player2Pos = player2.transform.position;
        Vector3 currentPos = transform.position;
        
        // Calculate distances to both players
        float distToPlayer1 = Vector3.Distance(currentPos, player1Pos);
        float distToPlayer2 = Vector3.Distance(currentPos, player2Pos);
        
        // Check for hits with either player
        if (distToPlayer1 <= hitDistance)
        {
            OnCollisionWithPlayer(player1);
            return;
        }
        if (distToPlayer2 <= hitDistance)
        {
            OnCollisionWithPlayer(player2);
            return;
        }
        
        // Calculate attraction forces to both players
        Vector3 attraction = Vector3.zero;
        
        // Add attraction to player1 if within radius
        if (distToPlayer1 < attractionRadius)
        {
            Vector3 toPlayer1 = (player1Pos - currentPos).normalized;
            float player1Strength = 1f - (distToPlayer1 / attractionRadius); // Stronger when closer
            attraction += toPlayer1 * player1Strength;
        }
        
        // Add attraction to player2 if within radius
        if (distToPlayer2 < attractionRadius)
        {
            Vector3 toPlayer2 = (player2Pos - currentPos).normalized;
            float player2Strength = 1f - (distToPlayer2 / attractionRadius); // Stronger when closer
            attraction += toPlayer2 * player2Strength;
        }
        
        // Apply attraction force
        Vector3 steeringForce = Vector3.zero;
        if (attraction != Vector3.zero)
        {
            Vector3 desiredVelocity = attraction.normalized * maxSpeed;
            steeringForce = Vector3.ClampMagnitude(desiredVelocity - velocity, maxSteerForce);
        }
        
        // Update velocity and position
        velocity += steeringForce * attractionWeight * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        
        // Ensure minimum speed
        if (velocity.magnitude < minSpeed)
        {
            velocity = velocity.normalized * minSpeed;
        }
        
        // Update position and rotation
        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;
        
        // Log status periodically
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[Predator] Status - Speed: {velocity.magnitude:F1}, " +
                     $"Dist to P1: {distToPlayer1:F1}, Dist to P2: {distToPlayer2:F1}");
        }
    }
    
    private void OnCollisionWithPlayer(GameObject player)
    {
        totalHits++;
        Debug.Log($"[Predator] Hit player {player.name}! Total hits: {totalHits}");
    }
    
    public int GetTotalHits()
    {
        return totalHits;
    }
}