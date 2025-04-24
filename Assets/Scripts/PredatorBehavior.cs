using UnityEngine;

public class PredatorBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 3.5f;  // Slightly slower than player speed
    
    [Tooltip("How fast the predator orbits around the target")]
    public float orbitSpeed = 120f;  // Degrees per second - faster orbit
    
    [Tooltip("How quickly the predator closes in on the target")]
    public float closingSpeed = 0.5f;  // Units per second - more aggressive closing
    
    [Tooltip("Minimum distance to maintain from target while orbiting")]
    public float minOrbitDistance = 1.5f;  // Get closer to player
    
    [Tooltip("Initial orbit radius when targeting a player")]
    public float initialOrbitRadius = 5f;  // Start circling from this distance
    
    [Tooltip("How quickly the predator moves to its orbit position")]
    public float orbitPositionSpeed = 5f;  // Quick movement to orbit position
    
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
            currentOrbitRadius = Mathf.Max(initialOrbitRadius, currentDistance);
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

            // If we're too far from target, move directly towards it first
            if (distanceToTarget > initialOrbitRadius * 1.5f)
            {
                Vector3 directPath = Vector3.MoveTowards(startPos, targetPos, moveSpeed * Time.deltaTime);
                transform.position = directPath;
                transform.LookAt(targetPos);
                
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[Predator] Moving directly to target. Distance: {distanceToTarget:F1}");
                }
                return;
            }

            // Update orbit angle - faster when closer to target
            float speedMultiplier = 1f + (1f - (currentOrbitRadius / initialOrbitRadius));
            orbitAngle += (orbitSpeed * speedMultiplier) * Time.deltaTime;
            if (orbitAngle >= 360f) orbitAngle -= 360f;
            
            // Calculate desired position on orbit
            float radian = orbitAngle * Mathf.Deg2Rad;
            Vector3 orbitOffset = new Vector3(
                Mathf.Cos(radian) * currentOrbitRadius,
                0f,
                Mathf.Sin(radian) * currentOrbitRadius
            );
            
            // Gradually reduce orbit radius, faster when player is moving
            Vector3 targetVelocity = (targetPos - currentTarget.transform.position) / Time.deltaTime;
            float closingMultiplier = 1f + (targetVelocity.magnitude * 0.1f);
            currentOrbitRadius = Mathf.Max(minOrbitDistance, 
                currentOrbitRadius - (closingSpeed * closingMultiplier * Time.deltaTime));
            
            // Calculate and move to desired position
            Vector3 desiredPosition = targetPos + orbitOffset;
            transform.position = Vector3.MoveTowards(
                transform.position,
                desiredPosition,
                orbitPositionSpeed * Time.deltaTime
            );

            // Look slightly ahead in the orbit for smoother rotation
            float lookAheadAngle = radian + (30f * Mathf.Deg2Rad);
            Vector3 lookAheadPoint = targetPos + new Vector3(
                Mathf.Cos(lookAheadAngle) * currentOrbitRadius,
                0f,
                Mathf.Sin(lookAheadAngle) * currentOrbitRadius
            );
            transform.LookAt(lookAheadPoint);

            // Log movement details every few frames
            if (Time.frameCount % 60 == 0)  // Log once per second at 60 fps
            {
                Debug.Log($"[Predator] Movement - Target: {currentTarget.name}, " +
                         $"Distance: {Vector3.Distance(transform.position, targetPos):F2}, " +
                         $"Orbit Radius: {currentOrbitRadius:F2}, " +
                         $"Speed: {moveSpeed:F2}, " +
                         $"Movement: {(transform.position - startPos).magnitude:F2}");
            }
        }
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
            currentOrbitRadius = initialOrbitRadius;
            Debug.Log($"[Predator] Switching target to {currentTarget.name}, starting orbit at radius {currentOrbitRadius:F1}");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == player1 || collision.gameObject == player2)
        {
            totalHits++;
            Debug.Log($"[Predator] Hit a player! Total hits: {totalHits}");
        }
    }
    
    public int GetTotalHits()
    {
        return totalHits;
    }
}