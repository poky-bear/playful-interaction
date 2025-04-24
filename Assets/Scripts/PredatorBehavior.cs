using UnityEngine;

public class PredatorBehavior : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float moveSpeed = 2f;  // Slower than player speed
    
    [Tooltip("How fast the predator orbits around the target")]
    public float orbitSpeed = 30f;  // Degrees per second
    
    [Tooltip("How quickly the predator closes in on the target")]
    public float closingSpeed = 0.1f;  // Units per second
    
    [Tooltip("Minimum distance to maintain from target while orbiting")]
    public float minOrbitDistance = 2f;
    
    [Header("References")]
    public GameObject player1;
    public GameObject player2;
    
    private GameObject currentTarget;
    private float currentOrbitRadius;
    private float orbitAngle;
    private int totalHits = 0;
    
    private void Start()
    {
        // Find the nearest player and set initial orbit radius
        UpdateTargetPlayer();
        if (currentTarget != null)
        {
            currentOrbitRadius = Vector3.Distance(transform.position, currentTarget.transform.position);
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
            // Calculate orbit position
            Vector3 targetPos = currentTarget.transform.position;
            
            // Update orbit angle
            orbitAngle += orbitSpeed * Time.deltaTime;
            if (orbitAngle >= 360f) orbitAngle -= 360f;
            
            // Calculate desired position on orbit
            float radian = orbitAngle * Mathf.Deg2Rad;
            Vector3 orbitOffset = new Vector3(
                Mathf.Cos(radian) * currentOrbitRadius,
                0f,
                Mathf.Sin(radian) * currentOrbitRadius
            );
            
            // Gradually reduce orbit radius
            currentOrbitRadius = Mathf.Max(minOrbitDistance, currentOrbitRadius - (closingSpeed * Time.deltaTime));
            
            // Set position and look at target
            Vector3 desiredPosition = targetPos + orbitOffset;
            transform.position = Vector3.MoveTowards(
                transform.position,
                desiredPosition,
                moveSpeed * Time.deltaTime
            );
            transform.LookAt(targetPos);
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
            Debug.Log($"[Predator] Switching target to {currentTarget.name}");
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