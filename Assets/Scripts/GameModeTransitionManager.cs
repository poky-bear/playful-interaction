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
    private GameObject player1;
    private GameObject player2;
    private MultiplayerRingGame multiplayerController;

    [Header("Predator Settings")]
    [Tooltip("Scale of the predator cube")]
    public Vector3 cubeScale = new Vector3(0.5f, 1f, 0.5f);
    [Tooltip("Color of the predator cube")]
    public Color cubeColor = Color.red;
    [Tooltip("Speed of the predator (should be less than player speed)")]
    public float predatorSpeed = 3f;
    [Tooltip("How fast the predator orbits around the target (degrees/second)")]
    public float orbitSpeed = 90f;
    [Tooltip("How quickly the predator closes in on the target")]
    public float closingSpeed = 0.2f;

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
        // Get references from the scene
        multiplayerController = GetComponentInParent<MultiplayerRingGame>();
        if (multiplayerController == null)
        {
            // Try finding it in the scene if not found in parent hierarchy
            multiplayerController = FindObjectOfType<MultiplayerRingGame>();
        }
        
        if (multiplayerController == null)
        {
            Debug.LogError("[Transition] MultiplayerRingGame component not found in scene! Please ensure there is a MultiplayerRingGame component in the scene.");
            return;
        }
        
        Debug.Log($"[Transition] Found MultiplayerRingGame component on {multiplayerController.gameObject.name}");

        // Get player references
        player1 = multiplayerController.player1Sphere;
        player2 = multiplayerController.player2Sphere;

        if (player1 == null || player2 == null)
        {
            Debug.LogError("[Transition] Player references not set in MultiplayerRingGame!");
            return;
        }

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

        // Spawn cube
        SpawnCube();

        Debug.Log("[Transition] Transition sequence complete");
        isTransitioning = false;
    }

    private void SpawnCube()
    {
        // Pick a random corner
        int randomCorner = Random.Range(0, cornerPositions.Length);
        Vector3 spawnPosition = cornerPositions[randomCorner];
        
        string[] cornerNames = new string[] { "Front Right", "Back Right", "Front Left", "Back Left" };
        Debug.Log($"[Transition] Spawning Predator cube in {cornerNames[randomCorner]} corner at position {spawnPosition}");

        // Create a cube primitive
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Predator";  // Name the cube "Predator"
        
        if (cube != null)
        {
            // Set position and scale
            cube.transform.position = spawnPosition;
            cube.transform.localScale = cubeScale;
            
            // Make the cube look at the center
            cube.transform.LookAt(Vector3.zero);
            
            // Set the color
            Renderer cubeRenderer = cube.GetComponent<Renderer>();
            if (cubeRenderer != null)
            {
                Material cubeMaterial = new Material(Shader.Find("Standard"));
                cubeMaterial.color = cubeColor;
                cubeRenderer.material = cubeMaterial;
            }
            
            // Add and configure the predator behavior
            PredatorBehavior predator = cube.AddComponent<PredatorBehavior>();
            predator.player1 = player1;
            predator.player2 = player2;
            predator.moveSpeed = predatorSpeed;
            predator.orbitSpeed = orbitSpeed;
            predator.closingSpeed = closingSpeed;
            
            Debug.Log($"[Transition] Predator cube spawned successfully and will begin hunting players");
        }
        else
        {
            Debug.LogError("[Transition] Failed to spawn Predator cube!");
        }
    }
}