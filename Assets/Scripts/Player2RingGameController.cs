using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2RingGameController : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Dark color for inactive rings")]
    public Color darkColor = new Color(0.1f, 0.1f, 0.1f, 1f); // Darker color for inactive rings
    
    [Tooltip("Bright color for active ring")]
    public Color brightColor = new Color(0.2f, 0.8f, 1f, 1f); // Bright blue for active ring (different from player 1)
    
    [Tooltip("Success message when all rings are completed")]
    public string successMessage = "Congratulations! Player 2 completed all rings!";
    
    [Tooltip("Speed at which the dark circle expands")]
    public float expansionSpeed = 1.0f;
    
    [Tooltip("Tolerance for hitting the active ring (smaller = more precise)")]
    public float hitTolerance = 0.0001f;

    [Header("References")]
    [Tooltip("Reference to the ConcentricRings component")]
    public ConcentricRings concentricRings;
    
    [Tooltip("Reference to the Cube object that will follow the sphere when game is completed")]
    public GameObject cubeObject;
    
    // Private variables
    private GameObject expandingCircle;
    private Material expandingCircleMaterial;
    private float currentRadius = 0f;
    private bool isExpanding = false;
    private int[] ringOrder = new int[3];
    private int currentRingIndex = 0;
    private bool gameCompleted = false;
    
    // Public properties for UI and other scripts
    public int CompletedRings { get { return currentRingIndex; } }
    public bool GameCompleted { get { return gameCompleted; } }
    private GameObject[] rings;
    private Material[] originalMaterials = new Material[3];
    private Material[] ringMaterials = new Material[3];

    void Start()
    {
        // Find ConcentricRings component if not assigned
        if (concentricRings == null)
        {
            concentricRings = GetComponent<ConcentricRings>();
            if (concentricRings == null)
            {
                Debug.LogError("No ConcentricRings component found on this GameObject!");
                return;
            }
        }
        
        // Find Cube object if not assigned
        if (cubeObject == null)
        {
            // Try to find a GameObject named "Player2Cube" in the scene
            cubeObject = GameObject.Find("Player2Cube");
            if (cubeObject == null)
            {
                Debug.LogWarning("No Player2Cube object found in the scene. Creating one...");
                // Create a cube if none exists
                cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeObject.name = "Player2Cube";
                // Position it away from the play area initially
                cubeObject.transform.position = new Vector3(0, -10, 0);
            }
        }

        // Wait for the ConcentricRings to initialize
        StartCoroutine(WaitForRingsInitialization());

        // Create expanding circle
        CreateExpandingCircle();
        
        // Initialize the game
        InitializeGame();
    }

    void CreateExpandingCircle()
    {
        // Create a sphere for the expanding circle
        expandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        expandingCircle.name = "Player2ExpandingCircle";
        expandingCircle.transform.parent = null; // Make it a root-level object like Player 1's
        
        // Position it at the sphere's position
        if (concentricRings != null && concentricRings.targetSphere != null)
        {
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        }
        else
        {
            expandingCircle.transform.localPosition = Vector3.zero;
        }
        
        // Start with zero scale
        expandingCircle.transform.localScale = Vector3.zero;
        
        // Remove collider as we don't need physics for this
        Destroy(expandingCircle.GetComponent<Collider>());
        
        // Create material for expanding circle with transparency
        expandingCircleMaterial = new Material(Shader.Find("Standard"));
        
        // Set up transparency
        expandingCircleMaterial.SetFloat("_Mode", 3); // Transparent mode
        expandingCircleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        expandingCircleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        expandingCircleMaterial.SetInt("_ZWrite", 0);
        expandingCircleMaterial.DisableKeyword("_ALPHATEST_ON");
        expandingCircleMaterial.EnableKeyword("_ALPHABLEND_ON");
        expandingCircleMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        expandingCircleMaterial.renderQueue = 3000;
        
        // Set a semi-transparent dark color
        expandingCircleMaterial.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0.5f);
        
        // Apply the material to the sphere
        expandingCircle.GetComponent<Renderer>().material = expandingCircleMaterial;
        
        // Hide it initially
        expandingCircle.SetActive(false);
        
        Debug.Log("Created Player 2 expanding circle");
    }

    void InitializeGame()
    {
        // Generate random order for the rings
        ringOrder = GenerateRandomOrder();
        currentRingIndex = 0;
        gameCompleted = false;
        
        // Set all rings to dark color initially
        for (int i = 0; i < 3; i++)
        {
            SetRingColor(i, darkColor);
        }
        
        // Set the first ring in order to bright
        SetRingColor(ringOrder[currentRingIndex], brightColor);
        
        // Reset the expanding circle
        if (expandingCircle != null)
        {
            expandingCircle.SetActive(false);
            currentRadius = 0f;
            isExpanding = false;
            
            // Reset the expanding circle position to the sphere's position
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
            expandingCircle.transform.localScale = Vector3.zero;
        }
        
        // Hide the cube at the start of the game
        if (cubeObject != null)
        {
            cubeObject.SetActive(false);
        }
        
        Debug.Log("Player 2 game initialized! Ring order: " + ringOrder[0] + ", " + ringOrder[1] + ", " + ringOrder[2]);
    }

    int[] GenerateRandomOrder()
    {
        int[] order = { 0, 1, 2 };
        
        // Fisher-Yates shuffle
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = order[i];
            order[i] = order[j];
            order[j] = temp;
        }
        
        return order;
    }

    void SetRingColor(int ringIndex, Color color)
    {
        if (ringIndex >= 0 && ringIndex < rings.Length && rings[ringIndex] != null)
        {
            ringMaterials[ringIndex].color = color;
            
            // Also set emission for bright color
            if (color == brightColor)
            {
                ringMaterials[ringIndex].EnableKeyword("_EMISSION");
                ringMaterials[ringIndex].SetColor("_EmissionColor", color * 0.5f);
            }
            else
            {
                ringMaterials[ringIndex].DisableKeyword("_EMISSION");
            }
        }
    }

    void Update()
    {
        if (gameCompleted)
        {
            // When game is completed, make the cube follow the sphere
            if (cubeObject != null && concentricRings != null && concentricRings.targetSphere != null)
            {
                // Turn on controller script for cube object
                cubeObject.transform.position = concentricRings.targetSphere.transform.position;
            }
            return;
        }
        
        // Check if multiplayer mode is active
        MultiplayerRingGame multiplayerGame = FindObjectOfType<MultiplayerRingGame>();
        bool isMultiplayerActive = multiplayerGame != null && multiplayerGame.MultiplayerModeActive;
        
        // Only handle individual player input if multiplayer mode is NOT active
        if (!isMultiplayerActive)
        {
            // Handle F key input instead of spacebar
            if (Input.GetKeyDown(KeyCode.F))
            {
                StartExpanding();
            }
            
            if (Input.GetKeyUp(KeyCode.F))
            {
                CheckHit();
            }
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            // Log that we're ignoring the individual player input due to multiplayer mode
            Debug.Log("Player 2: Ignoring individual circle expansion because multiplayer mode is active");
        }
        
        // Update expanding circle
        if (isExpanding)
        {
            // Increase the radius based on time
            currentRadius += expansionSpeed * Time.deltaTime;
            
            // Update the scale of the expanding circle
            expandingCircle.transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);
            
            // Ensure the expanding circle stays centered on the sphere
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        }
        
        // Always keep the expanding circle centered on the sphere, even when not expanding
        // This ensures the distance calculation is always relative to the sphere's current position
        if (expandingCircle.activeSelf)
        {
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        }
    }
    
    void StartExpanding()
    {
        isExpanding = true;
        currentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle at the sphere's current position
        expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
        
        // Set the expanding circle material to dark color
        expandingCircleMaterial.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0.5f);
        
        // Activate the expanding circle
        expandingCircle.SetActive(true);
        
        Debug.Log("Player 2 started expanding circle");
    }
    
    void CheckHit()
    {
        isExpanding = false;
        
        // Get the current active ring radius
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        
        // Calculate the distance from the sphere's center to the expanding circle edge
        // This ensures we're measuring from the sphere's position, not world origin
        Vector3 spherePosition = concentricRings.targetSphere.transform.position;
        float distanceFromSphereToCircleEdge = currentRadius / 2;
        
        // Calculate how close the player was to the target
        // The distance should be measured relative to the sphere's position
        float distanceFromTarget = Mathf.Abs(distanceFromSphereToCircleEdge - activeRingRadius);
        
        Debug.Log("Player 2 - Radius of target ring: " + activeRingRadius + 
                ", Radius of user ring: " + distanceFromSphereToCircleEdge + 
                ", total diff: " + distanceFromTarget);

        // Show visual feedback of how close they were
        StartCoroutine(ShowHitFeedback(distanceFromTarget, activeRingRadius));
        
        // Check if the expanding circle is close to the active ring
        if (distanceFromTarget < hitTolerance)
        {
            // Success! Move to the next ring
            SetRingColor(ringOrder[currentRingIndex], darkColor);
            currentRingIndex++;
            
            if (currentRingIndex >= ringOrder.Length)
            {
                // Game completed!
                Debug.Log("Player 2: " + successMessage);
                gameCompleted = true;
                
                // Hide the expanding circle
                expandingCircle.SetActive(false);
                currentRadius = 0f;
                
                // Notify UI if available
                Player2RingGameUI ui = GetComponent<Player2RingGameUI>();
                if (ui != null)
                {
                    ui.ShowGameCompleteMessage();
                }
            }
            else
            {
                // Activate the next ring in the order
                SetRingColor(ringOrder[currentRingIndex], brightColor);
                Debug.Log("Player 2 good hit! Moving to next ring: " + ringOrder[currentRingIndex]);
                
                // Reset the expanding circle for the next attempt
                expandingCircle.SetActive(false);
                currentRadius = 0f;
                
                // Update UI if available
                Player2RingGameUI ui = GetComponent<Player2RingGameUI>();
                if (ui != null)
                {
                    ui.ShowHitFeedback("Good hit!", Color.green);
                }
            }
        }
        else
        {
            // Provide feedback on how close they were
            if (distanceFromTarget < hitTolerance * 2)
            {
                Debug.Log("Player 2 close! You were " + distanceFromTarget.ToString("F2") + " units away. Tolerance is " + hitTolerance + " units.");
                
                // Update UI if available
                Player2RingGameUI ui = GetComponent<Player2RingGameUI>();
                if (ui != null)
                {
                    ui.ShowHitFeedback("Close! Try again", new Color(1f, 0.6f, 0f)); // Orange
                }
            }
            else
            {
                Debug.Log("Player 2 miss! You were " + distanceFromTarget.ToString("F2") + " units away. Tolerance is " + hitTolerance + " units.");
                
                // Update UI if available
                Player2RingGameUI ui = GetComponent<Player2RingGameUI>();
                if (ui != null)
                {
                    ui.ShowHitFeedback("Miss! Try again", Color.red);
                }
            }
        }
    }
    
    // Coroutine to show visual feedback of how close the hit was
    private System.Collections.IEnumerator ShowHitFeedback(float distanceFromTarget, float targetRadius)
    {
        // Keep the expanding circle visible for feedback
        Color feedbackColor;
        
        if (distanceFromTarget < hitTolerance)
        {
            // Good hit - green
            feedbackColor = Color.green;
        }
        else if (distanceFromTarget < hitTolerance * 2)
        {
            // Close - orange
            feedbackColor = new Color(1f, 0.6f, 0f);
        }
        else
        {
            // Miss - red
            feedbackColor = Color.red;
        }
        
        // Set the color with transparency
        expandingCircleMaterial.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0.5f);
        
        // Flash the expanding circle
        float duration = 0.5f;
        float time = 0;
        
        // Save the current radius for the feedback animation
        float feedbackRadius = currentRadius;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0.1f, time / duration);
            
            expandingCircleMaterial.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, alpha);
            
            // Keep the circle at the same size during feedback
            expandingCircle.transform.localScale = new Vector3(feedbackRadius, feedbackRadius, feedbackRadius);
            
            // Ensure the circle stays centered on the sphere even if it's moving
            expandingCircle.transform.position = concentricRings.targetSphere.transform.position;
            
            yield return null;
        }
        
        // Only hide the expanding circle if we're not moving to the next ring
        // (for successful hits, we already hide it in CheckHit)
        if (distanceFromTarget >= hitTolerance)
        {
            // Hide the expanding circle
            expandingCircle.SetActive(false);
            
            // Reset the expanding circle for the next attempt
            currentRadius = 0f;
        }
        
        // Reset the expanding circle material color back to dark
        expandingCircleMaterial.color = new Color(darkColor.r, darkColor.g, darkColor.b, 0.5f);
    }
    
    float GetRingRadius(int ringIndex)
    {
        // Use the same radius calculation as in ConcentricRings.CreateRings
        // This ensures we're using the exact same radius values
        return concentricRings.sphereRadius + concentricRings.minDistanceToFirstRing + (ringIndex * concentricRings.ringSpacing);
    }

    // Reset the game
    public void ResetGame()
    {
        // Hide the cube when resetting the game
        if (cubeObject != null)
        {
            cubeObject.SetActive(false);
        }

        InitializeGame();
    }

    // Coroutine to wait for rings to be initialized by ConcentricRings
    private System.Collections.IEnumerator WaitForRingsInitialization()
    {
        // Wait for the ConcentricRings to create the rings
        yield return new WaitForSeconds(0.2f);

        // Get references to the rings
        rings = new GameObject[3];

        // Use the rings array from ConcentricRings if available
        if (concentricRings.rings[0] != null)
        {
            for (int i = 0; i < 3; i++)
            {
                rings[i] = concentricRings.rings[i];

                // Store original materials
                if (rings[i] != null)
                {
                    Renderer renderer = rings[i].GetComponent<Renderer>();
                    originalMaterials[i] = renderer.material;

                    // Create a new material instance to avoid modifying the original
                    ringMaterials[i] = new Material(originalMaterials[i]);
                    renderer.material = ringMaterials[i];
                }

                // Log the ring radius for debugging
                float ringRadius = GetRingRadius(i);
                Debug.Log("Player 2 Ring " + i + " radius: " + ringRadius);
            }

            // Log sphere radius for debugging
            Debug.Log("Player 2 Sphere radius from ConcentricRings: " + concentricRings.sphereRadius);

            // Initialize the game once rings are ready
            InitializeGame();
        }
        else
        {
            Debug.LogError("Player 2: Rings not found in ConcentricRings component!");
        }
    }
}