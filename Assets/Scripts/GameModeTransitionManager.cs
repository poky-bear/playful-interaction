using UnityEngine;
using System.Collections;

public class GameModeTransitionManager : MonoBehaviour
{
    [System.Serializable]
    public class TransitionSettings
    {
        [Header("Timing")]
        public float initialWaitTime = 10f;
        public float speedTransitionDuration = 5f;

        [Header("Speed Settings")]
        public float maxSpeedIncrease = 10f;

        [Header("Spawn Settings")]
        public float cornerOffset = 10f;
    }

    [Header("Settings")]
    public TransitionSettings settings = new TransitionSettings();

    [Header("References")]
    public BoidSettings boidSettings;
    public GameObject conePrefab;

    private BoidManager boidManager;
    private float originalMinSpeed;
    private float originalMaxSpeed;
    private bool isTransitioning;
    private Vector3[] cornerPositions;

    private void Awake()
    {
        // Initialize corner positions
        cornerPositions = new Vector3[4]
        {
            new Vector3(settings.cornerOffset, 0, settings.cornerOffset),   // Front Right
            new Vector3(settings.cornerOffset, 0, -settings.cornerOffset),  // Back Right
            new Vector3(-settings.cornerOffset, 0, settings.cornerOffset),  // Front Left
            new Vector3(-settings.cornerOffset, 0, -settings.cornerOffset)  // Back Left
        };
    }

    private void Start()
    {
        // Find BoidManager if not assigned
        if (boidManager == null)
        {
            boidManager = FindObjectOfType<BoidManager>();
        }

        // Get BoidSettings from BoidManager if not assigned
        if (boidSettings == null && boidManager != null)
        {
            boidSettings = boidManager.settings;
        }

        if (boidSettings == null)
        {
            Debug.LogError("BoidSettings not found! Please assign BoidSettings in the inspector or ensure BoidManager exists in the scene.");
            return;
        }

        // Store original speeds
        originalMinSpeed = boidSettings.minSpeed;
        originalMaxSpeed = boidSettings.maxSpeed;
    }

    public void StartTransition()
    {
        if (!isTransitioning && boidSettings != null)
        {
            StartCoroutine(TransitionSequence());
        }
        else if (boidSettings == null)
        {
            Debug.LogError("Cannot start transition: BoidSettings is null!");
        }
    }

    private IEnumerator TransitionSequence()
    {
        isTransitioning = true;
        Debug.Log("[Transition] Starting transition sequence - waiting for " + settings.initialWaitTime + " seconds");

        // Initial wait
        yield return new WaitForSeconds(settings.initialWaitTime);

        // Store initial speeds
        float initialMinSpeed = boidSettings.minSpeed;
        float initialMaxSpeed = boidSettings.maxSpeed;
        Debug.Log("[Transition] Starting speed increase - Initial speeds: Min=" + initialMinSpeed + ", Max=" + initialMaxSpeed);

        // Gradually increase speed
        float elapsedTime = 0f;
        while (elapsedTime < settings.speedTransitionDuration)
        {
            float t = elapsedTime / settings.speedTransitionDuration;
            float speedIncrease = t * settings.maxSpeedIncrease;
            
            boidSettings.minSpeed = initialMinSpeed + speedIncrease;
            boidSettings.maxSpeed = initialMaxSpeed + speedIncrease;
            
            // Log speed changes at 25%, 50%, and 75% of the transition
            if (Mathf.Approximately(t, 0.25f) || Mathf.Approximately(t, 0.5f) || Mathf.Approximately(t, 0.75f))
            {
                Debug.Log($"[Transition] Speed increase progress {t*100}% - Current speeds: Min={boidSettings.minSpeed:F2}, Max={boidSettings.maxSpeed:F2}");
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("[Transition] Speed increase complete - Resetting to original speeds");
        
        // Reset speeds
        boidSettings.minSpeed = originalMinSpeed;
        boidSettings.maxSpeed = originalMaxSpeed;

        // Spawn cone
        SpawnCone();

        Debug.Log("[Transition] Transition sequence complete");
        isTransitioning = false;
    }

    private void SpawnCone()
    {
        if (conePrefab == null)
        {
            Debug.LogError("[Transition] Cannot spawn cone: conePrefab is not assigned!");
            return;
        }

        // Pick a random corner
        int randomCorner = Random.Range(0, cornerPositions.Length);
        Vector3 spawnPosition = cornerPositions[randomCorner];
        
        string[] cornerNames = new string[] { "Front Right", "Back Right", "Front Left", "Back Left" };
        Debug.Log($"[Transition] Spawning cone in {cornerNames[randomCorner]} corner at position {spawnPosition}");

        // Instantiate and orient the cone
        GameObject cone = Instantiate(conePrefab, spawnPosition, Quaternion.identity);
        if (cone != null)
        {
            cone.transform.LookAt(Vector3.zero);
            cone.transform.Rotate(90f, 0f, 0f);
            Debug.Log($"[Transition] Cone spawned successfully and oriented towards center");
        }
        else
        {
            Debug.LogError("[Transition] Failed to spawn cone!");
        }
    }
}