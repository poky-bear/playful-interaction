using UnityEngine;

public class PredatorBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 3.5f;  // Slightly slower than player speed
    
    [Tooltip("Distance at which predator starts orbiting")]
    public float orbitStartDistance = 5f;  // When to start orbiting
    
    [Tooltip("How fast the predator orbits around the target")]
    public float orbitSpeed = 180f;  // Degrees per second - faster orbit
    
    [Tooltip("How quickly the predator closes in on the target")]
    public float closingSpeed = 0.8f;  // Units per second - aggressive closing
    
    [Tooltip("Minimum distance before considering it a 'hit'")]
    public float hitDistance = 0.5f;  // Distance to count as touching
    
    [Header("References")]
    public GameObject player1;
    public GameObject player2;
    
    private GameObject currentTarget;
    private float currentOrbitRadius;
    private float orbitAngle;
    private int totalHits = 0;
    
    private void Start()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogError("[Predator] Player references not set! Player1: " + (player1 != null) + ", Player2: " + (player2 != null));
            return;
        }

        Debug.Log($"[Predator] Initialized with Player1: {player1.name}, Player2: {player2.name}");
        
        // Find the nearest player and set initial orbit radius
        UpdateTargetPlayer();
        if (currentTarget != null)
        {
            // Start at the initial orbit radius or current distance, whichever is larger
            float currentDistance = Vector3.Distance(transform.position, currentTarget.transform.position);
            currentOrbitRadius = Mathf.Max(orbitStartDistance, currentDistance);
            Debug.Log($"[Predator] Initial orbit radius set to {currentOrbitRadius:F1} units");
        }
        
        Debug.Log($"[Predator] Starting to track players. Initial target: {(currentTarget != null ? currentTarget.name : "none")}");
    }
    
    private void Update()
    {
        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("[Predator] One or both players are missing!");
            return;
        }
        
        // Periodically check if we should switch targets
        UpdateTargetPlayer();
        
        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 startPos = transform.position;
            float distanceToTarget = Vector3.Distance(startPos, targetPos);

            // Check if we've hit the player
            if (distanceToTarget <= hitDistance)
            {
                OnCollisionWithPlayer(currentTarget);
                return;
            }

            // If we're outside orbit range, move directly towards target
            if (distanceToTarget > orbitStartDistance)
            {
                // Direct pursuit mode
                Vector3 directPath = Vector3.MoveTowards(startPos, targetPos, moveSpeed * Time.deltaTime);
                transform.position = directPath;
                transform.LookAt(targetPos);
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Predator] Direct pursuit. Distance: {distanceToTarget:F1}");
                }
            }
            else
            {
                // Orbital attack mode
                // Update orbit angle - faster when closer to target
                float speedMultiplier = 1f + ((orbitStartDistance - distanceToTarget) / orbitStartDistance);
                orbitAngle += (orbitSpeed * speedMultiplier) * Time.deltaTime;
                if (orbitAngle >= 360f) orbitAngle -= 360f;
                
                // Calculate current orbit radius and desired position
                float currentRadius = distanceToTarget;
                float radian = orbitAngle * Mathf.Deg2Rad;
                Vector3 orbitOffset = new Vector3(
                    Mathf.Cos(radian) * currentRadius,
                    0f,
                    Mathf.Sin(radian) * currentRadius
                );
                
                // Calculate next position, moving both around and towards the target
                Vector3 orbitPosition = targetPos + orbitOffset;
                Vector3 nextPos = Vector3.MoveTowards(
                    orbitPosition, 
                    targetPos, 
                    closingSpeed * Time.deltaTime
                );
                
                // Move to the calculated position
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    nextPos,
                    moveSpeed * Time.deltaTime
                );
                
                // Always look at target during orbital attack
                transform.LookAt(targetPos);
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Predator] Orbital attack. Distance: {distanceToTarget:F1}, Speed Multiplier: {speedMultiplier:F1}");
                }
            }
        }
    }
    
    private void OnCollisionWithPlayer(GameObject player)
    {
        totalHits++;
        Debug.Log($"[Predator] Hit player {player.name}! Total hits: {totalHits}");
    }
    
    private void UpdateTargetPlayer()
    {
        if (player1 == null || player2 == null) return;
        
        float distToPlayer1 = Vector3.Distance(transform.position, player1.transform.position);
        float distToPlayer2 = Vector3.Distance(transform.position, player2.transform.position);
        
        GameObject newTarget = (distToPlayer1 <= distToPlayer2) ? player1 : player2;
        
        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            // Reset orbit radius when switching targets to start the circling pattern again
            currentOrbitRadius = orbitStartDistance;
            Debug.Log($"[Predator] Switching target to {currentTarget.name}, starting orbit at radius {currentOrbitRadius:F1}");
        }
    }
    
    // Using distance-based hit detection instead of physical collisions
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (collision.gameObject == player1 || collision.gameObject == player2)
    //     {
    //         totalHits++;
    //         Debug.Log($"[Predator] Hit a player! Total hits: {totalHits}");
    //     }
    // }
    
    public int GetTotalHits()
    {
        return totalHits;
    }
}