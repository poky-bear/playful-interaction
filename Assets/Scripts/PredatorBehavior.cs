using UnityEngine;

public class PredatorBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Layer mask for obstacle detection")]
    public LayerMask obstacleMask;
    
    [Tooltip("Radius for obstacle detection")]
    public float boundsRadius = 0.5f;
    
    [Tooltip("Distance to look ahead for obstacles")]
    public float collisionAvoidDst = 3f;
    
    [Tooltip("Weight of obstacle avoidance")]
    public float avoidCollisionWeight = 2f;
    [Tooltip("Maximum movement speed")]
    public float maxSpeed = 4f;  // Slower than boids
    
    [Tooltip("Minimum movement speed")]
    public float minSpeed = 2f;  // Keep some movement
    
    [Tooltip("How strongly the predator steers")]
    public float maxSteerForce = 2f;  // Gentle steering
    
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
    
    [Header("Game Settings")]
    [Tooltip("Reference to the predator ring game controller")]
    public PredatorRingGame ringGame;
    
    [Tooltip("If true, the game won't end when the predator catches a player")]
    public bool disableGameOver = true;

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

        // Find or create ring game controller
        if (ringGame == null)
        {
            GameObject ringGameObj = new GameObject("PredatorRingGame");
            ringGame = ringGameObj.AddComponent<PredatorRingGame>();
            ringGame.player1Sphere = player1;
            ringGame.player2Sphere = player2;
            ringGame.predator = gameObject;
            ringGame.OnPredatorModeActivated();
        }
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
        
        // Calculate target position based on closest player
        Vector3 targetPos;
        bool player1InRange = distToPlayer1 < attractionRadius;
        bool player2InRange = distToPlayer2 < attractionRadius;
        
        if (player1InRange || player2InRange) {
            // Target the closest player if at least one is in range
            targetPos = (distToPlayer1 <= distToPlayer2) ? player1Pos : player2Pos;
        } else {
            // If no players in range, maintain current velocity
            targetPos = transform.position + velocity;
        }

        // Calculate desired velocity towards target
        Vector3 offsetToTarget = (targetPos - currentPos);
        Vector3 desiredVelocity = offsetToTarget.normalized * maxSpeed;
        
        // Calculate steering force
        Vector3 steeringForce = Vector3.ClampMagnitude(desiredVelocity - velocity, maxSteerForce);
        
        // Apply smoother acceleration using attractionWeight
        Vector3 acceleration = steeringForce * attractionWeight;
        
        // Check for obstacles and avoid if necessary
        if (IsHeadingForCollision()) {
            Vector3 collisionAvoidDir = ObstacleRays();
            Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        // Update velocity with smoothing
        velocity = Vector3.Lerp(velocity, velocity + acceleration, Time.deltaTime * 5f);
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

    private bool IsHeadingForCollision() {
        RaycastHit hit;
        return Physics.SphereCast(transform.position, boundsRadius, velocity.normalized, 
            out hit, collisionAvoidDst, obstacleMask);
    }

    private Vector3 ObstacleRays() {
        Vector3[] rayDirections = new Vector3[] {
            transform.up,
            transform.up + transform.right,
            transform.up - transform.right,
            transform.right,
            -transform.right,
            -transform.up + transform.right,
            -transform.up - transform.right,
            -transform.up
        };

        for (int i = 0; i < rayDirections.Length; i++) {
            Vector3 dir = rayDirections[i].normalized;
            Ray ray = new Ray(transform.position, dir);
            if (!Physics.SphereCast(ray, boundsRadius, collisionAvoidDst, obstacleMask)) {
                return dir;
            }
        }

        return transform.forward;
    }

    private Vector3 SteerTowards(Vector3 vector) {
        if (vector.sqrMagnitude < 0.000001f) {
            return Vector3.zero;
        }
        Vector3 v = vector.normalized * maxSpeed - velocity;
        return Vector3.ClampMagnitude(v, maxSteerForce);
    }
    
    private void OnCollisionWithPlayer(GameObject player)
    {
        // If game over is disabled, just count the hit and return
        if (!disableGameOver)
        {
            totalHits++;
            Debug.Log($"[Predator] Hit player {player.name}! Game Over!");
            
            // Deactivate ring game
            if (ringGame != null)
            {
                ringGame.OnPredatorModeDeactivated();
            }
            
            // Find GameManager and end the game
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.EndGame($"Game Over! {player.name} was caught!");
            }
            else
            {
                Debug.LogWarning("[Predator] GameManager not found! Stopping time instead.");
                Time.timeScale = 0; // Pause the game if no GameManager is found
            }
        }
        else
        {
            totalHits++;
            Debug.Log($"[Predator] Hit player {player.name}! (Game Over disabled)");
        }
    }
    
    public int GetTotalHits()
    {
        return totalHits;
    }
}