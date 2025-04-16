using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerRingGame : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [Tooltip("Distance at which multiplayer mode activates")]
    public float activationDistance = 3.0f;
    
    [Tooltip("Time players need to stay close to activate multiplayer mode")]
    public float activationTime = 3.0f;
    
    [Tooltip("Color for the multiplayer ring")]
    public Color multiplayerRingColor = new Color(0.8f, 0.2f, 0.8f, 1f); // Purple color for multiplayer
    
    [Tooltip("Success message when all rings are completed")]
    public string successMessage = "Congratulations! Both players completed the challenge!";
    
    [Header("References")]
    [Tooltip("Reference to Player 1 sphere")]
    public GameObject player1Sphere;
    
    [Tooltip("Reference to Player 2 sphere")]
    public GameObject player2Sphere;
    
    [Tooltip("Reference to the multiplayer ring object")]
    public GameObject multiplayerRingObject = null;
    
    // Private variables
    private float proximityTimer = 0f;
    private bool multiplayerModeActive = false;
    private bool gameCompleted = false;
    private GameObject player1ExpandingCircle = null;
    private GameObject player2ExpandingCircle = null;
    private Material player1ExpandingCircleMaterial;
    private Material player2ExpandingCircleMaterial;
    private float player1CurrentRadius = 0f;
    private float player2CurrentRadius = 0f;
    private bool player1IsExpanding = false;
    private bool player2IsExpanding = false;
    private int[] ringOrder = new int[3];
    private int currentRingIndex = 0;
    private GameObject[] rings;
    private Material[] ringMaterials = new Material[3];
    private Material[] originalMaterials = new Material[3];
    private RingGameController player1Controller;
    private Player2RingGameController player2Controller;
    private bool player1Ready = false;
    private bool player2Ready = false;
    
    // Variables for synchronized ring completion
    private bool player1Success = false;
    private bool player2Success = false;
    private float player1Distance = 0f;
    private float player2Distance = 0f;
    
    // Public properties for UI
    public bool MultiplayerModeActive { get { return multiplayerModeActive; } }
    public bool GameCompleted { get { return gameCompleted; } }
    public int CompletedRings { get { return currentRingIndex; } }
    
    // Get the percentage of proximity timer compared to activation time
    public float GetProximityPercentage()
    {
        return proximityTimer / activationTime;
    }
    
    void Start()
    {
        // Find player spheres if not assigned
        if (player1Sphere == null)
        {
            player1Sphere = GameObject.Find("Sphere");
            if (player1Sphere == null)
            {
                Debug.LogError("Player 1 sphere not found!");
            }
        }
        
        if (player2Sphere == null)
        {
            player2Sphere = GameObject.Find("Player2Sphere");
            if (player2Sphere == null)
            {
                Debug.LogError("Player 2 sphere not found!");
            }
        }
        
        // Get references to the ring game controllers
        if (player1Sphere != null)
        {
            player1Controller = player1Sphere.GetComponent<RingGameController>();
            if (player1Controller == null)
            {
                Debug.LogError("RingGameController not found on Player 1 sphere!");
            }
        }
        
        if (player2Sphere != null)
        {
            player2Controller = player2Sphere.GetComponent<Player2RingGameController>();
            if (player2Controller == null)
            {
                Debug.LogError("Player2RingGameController not found on Player 2 sphere!");
            }
        }
        
        // Create expanding circles
        CreateExpandingCircles();
    }
    
    void CreateMultiplayerRing()
    {
        // Create a parent object for the multiplayer rings
        multiplayerRingObject = new GameObject("MultiplayerRings");
        
        // Position at the midpoint between players if they exist
        if (player1Sphere != null && player2Sphere != null)
        {
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            multiplayerRingObject.transform.position = midpoint;
        }
        
        // Create rings
        rings = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            // Use cylinder for rings instead of sphere for better visual appearance
            rings[i] = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rings[i].name = "MultiplayerRing" + i;
            rings[i].transform.parent = multiplayerRingObject.transform;
            
            // Rotate to make it a horizontal ring
            rings[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            // Make it thin (like a ring)
            rings[i].transform.localScale = new Vector3(1, 0.05f, 1);
            
            // Remove collider as we don't need physics for the rings
            Destroy(rings[i].GetComponent<Collider>());
            
            // Create material for the ring
            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.color = Color.black; // Start with black color
            
            // Store the material
            originalMaterials[i] = ringMaterial;
            ringMaterials[i] = new Material(ringMaterial);
            
            // Apply the material to the ring
            rings[i].GetComponent<Renderer>().material = ringMaterials[i];
        }
        
        // Hide the rings initially
        multiplayerRingObject.SetActive(false);
    }
    
    void CreateExpandingCircles()
    {
        // Create Player 1's expanding circle
        player1ExpandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player1ExpandingCircle.name = "Player1ExpandingCircle";
        player1ExpandingCircle.transform.localScale = Vector3.zero;
        player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        Destroy(player1ExpandingCircle.GetComponent<Collider>());
        
        // Create material for Player 1's expanding circle
        player1ExpandingCircleMaterial = new Material(Shader.Find("Standard"));
        SetupTransparentMaterial(player1ExpandingCircleMaterial);
        player1ExpandingCircleMaterial.color = new Color(1f, 0.8f, 0.2f, 0.5f); // Yellow tint
        player1ExpandingCircle.GetComponent<Renderer>().material = player1ExpandingCircleMaterial;
        player1ExpandingCircle.SetActive(false);
        
        // Create Player 2's expanding circle
        player2ExpandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player2ExpandingCircle.name = "Player2ExpandingCircle";
        player2ExpandingCircle.transform.localScale = Vector3.zero;
        player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        Destroy(player2ExpandingCircle.GetComponent<Collider>());
        
        // Create material for Player 2's expanding circle
        player2ExpandingCircleMaterial = new Material(Shader.Find("Standard"));
        SetupTransparentMaterial(player2ExpandingCircleMaterial);
        player2ExpandingCircleMaterial.color = new Color(0.2f, 0.8f, 1f, 0.5f); // Blue tint
        player2ExpandingCircle.GetComponent<Renderer>().material = player2ExpandingCircleMaterial;
        player2ExpandingCircle.SetActive(false);
    }
    
    void SetupTransparentMaterial(Material material)
    {
        material.SetFloat("_Mode", 3); // Transparent mode
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    void Update()
    {
        if (player1Sphere == null || player2Sphere == null)
            return;
            
        if (gameCompleted)
            return;
        
        // Always check the distance between players
        float distance = Vector3.Distance(player1Sphere.transform.position, player2Sphere.transform.position);
            
        // Check if multiplayer mode is already active
        if (multiplayerModeActive)
        {
            // Check if players have moved too far apart
            if (distance > activationDistance * 1.5f)
            {
                // Players moved too far apart, deactivate multiplayer mode
                DeactivateMultiplayerMode();
                Debug.Log("Players moved too far apart. Distance: " + distance + ", Deactivating multiplayer mode.");
            }
            else
            {
                // Continue with multiplayer game
                UpdateMultiplayerGame();
            }
        }
        else
        {
            // Check if players are close enough to activate multiplayer mode
            if (distance <= activationDistance)
            {
                // Players are close, increment timer
                proximityTimer += Time.deltaTime;
                
                // Check if timer has reached the activation time
                if (proximityTimer >= activationTime)
                {
                    // Activate multiplayer mode
                    ActivateMultiplayerMode();
                }
            }
            else
            {
                // Reset timer if players move apart
                proximityTimer = 0f;
            }
        }
    }
    
    void UpdateMultiplayerGame()
    {
        // Check for space bar press (Player 1)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player1Ready = true;
            // Start expanding circle when spacebar is pressed
            StartPlayer1Expanding();
        }
        
        // Check for space bar release
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player1Ready = false;
            // When space bar is released, check hit for Player 1
            if (player1IsExpanding)
            {
                CheckPlayer1Hit();
            }
        }
        
        // Check for F key press (Player 2)
        if (Input.GetKeyDown(KeyCode.F))
        {
            player2Ready = true;
            // Start expanding circle when F key is pressed
            StartPlayer2Expanding();
        }
        
        // Check for F key release
        if (Input.GetKeyUp(KeyCode.F))
        {
            player2Ready = false;
            // When F key is released, check hit for Player 2
            if (player2IsExpanding)
            {
                CheckPlayer2Hit();
            }
        }
        
        // Update expanding circles if active
        if (player1IsExpanding || player2IsExpanding)
        {
            // Update Player 1's expanding circle
            if (player1IsExpanding)
            {
                player1CurrentRadius += 1.0f * Time.deltaTime;
                player1ExpandingCircle.transform.localScale = new Vector3(player1CurrentRadius, player1CurrentRadius, player1CurrentRadius);
                player1ExpandingCircle.transform.position = player1Sphere.transform.position;
            }
            
            // Update Player 2's expanding circle
            if (player2IsExpanding)
            {
                player2CurrentRadius += 1.0f * Time.deltaTime;
                player2ExpandingCircle.transform.localScale = new Vector3(player2CurrentRadius, player2CurrentRadius, player2CurrentRadius);
                player2ExpandingCircle.transform.position = player2Sphere.transform.position;
            }
            
            // Keep the multiplayer ring object at the midpoint
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            multiplayerRingObject.transform.position = midpoint;
        }
    }
    
    void ActivateMultiplayerMode()
    {
        multiplayerModeActive = true;
        Debug.Log("Multiplayer mode activated!");
        
        // First, deactivate the original rings around both spheres
        DeactivateOriginalRings();
        
        // Create multiplayer ring object if not assigned
        if (multiplayerRingObject == null)
        {
            CreateMultiplayerRing();
        }
        
        // Position the multiplayer rings at the midpoint between the two players
        Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        multiplayerRingObject.transform.position = midpoint;
        
        // Find and hide any active individual expanding circles
        HideIndividualExpandingCircles();
        
        // Set up the rings
        float baseRadius = 2.0f; // Base radius for the first ring
        float ringSpacing = 1.0f; // Spacing between rings
        
        for (int i = 0; i < 3; i++)
        {
            float radius = baseRadius + (i * ringSpacing);
            // For cylinders, we need to set the x and z scale for the radius
            rings[i].transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2); // Make rings thin
            
            // Set all rings to dark initially
            SetRingColor(i, Color.black);
        }
        
        // Generate random order for the rings
        ringOrder = GenerateRandomOrder();
        currentRingIndex = 0;
        
        // Set the first ring to the multiplayer color
        SetRingColor(ringOrder[currentRingIndex], multiplayerRingColor);
        
        // Show the multiplayer rings
        multiplayerRingObject.SetActive(true);
        
        // Reset player states
        player1Ready = false;
        player2Ready = false;
        player1Success = false;
        player2Success = false;
        player1Distance = 0f;
        player2Distance = 0f;
    }
    
    void HideIndividualExpandingCircles()
    {
        // Find and hide Player 1's expanding circle
        GameObject player1ExpandingCircle = GameObject.Find("ExpandingCircle");
        if (player1ExpandingCircle != null)
        {
            player1ExpandingCircle.SetActive(false);
            Debug.Log("Hid Player 1's expanding circle");
        }
        
        // Find and hide Player 2's expanding circle (it might have a different name)
        GameObject player2ExpandingCircle = null;
        
        // Try to find it through the Player2RingGameController
        Player2RingGameController player2Controller = player2Sphere.GetComponent<Player2RingGameController>();
        if (player2Controller != null)
        {
            // Look for a child object with "Expanding" in the name
            foreach (Transform child in player2Controller.transform)
            {
                if (child.name.Contains("Expanding"))
                {
                    player2ExpandingCircle = child.gameObject;
                    break;
                }
            }
        }
        
        // If we couldn't find it that way, try a more general search
        if (player2ExpandingCircle == null)
        {
            // Try to find any other expanding circles
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Expanding") && obj != player1ExpandingCircle)
                {
                    player2ExpandingCircle = obj;
                    break;
                }
            }
        }
        
        if (player2ExpandingCircle != null)
        {
            player2ExpandingCircle.SetActive(false);
            Debug.Log("Hid Player 2's expanding circle: " + player2ExpandingCircle.name);
        }
        else
        {
            Debug.Log("Could not find Player 2's expanding circle");
        }
    }
    
    void DeactivateOriginalRings()
    {
        Debug.Log("Deactivating original rings for multiplayer mode...");
        
        // Deactivate Player 1's rings
        if (player1Sphere != null)
        {
            ConcentricRings player1Rings = player1Sphere.GetComponent<ConcentricRings>();
            if (player1Rings != null && player1Rings.rings != null)
            {
                int deactivatedCount = 0;
                foreach (GameObject ring in player1Rings.rings)
                {
                    if (ring != null)
                    {
                        ring.SetActive(false);
                        deactivatedCount++;
                    }
                }
                Debug.Log("Deactivated " + deactivatedCount + " rings for Player 1");
                
                // Also disable the ConcentricRings component to prevent it from reactivating the rings
                player1Rings.enabled = false;
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 1 sphere");
            }
            
            // Also disable the RingGameController to prevent it from interfering with multiplayer mode
            if (player1Controller != null)
            {
                player1Controller.enabled = false;
                // Don't deactivate the gameObject as it's the player sphere itself
                Debug.Log("Disabled RingGameController on Player 1");
            }
        }
        else
        {
            Debug.LogWarning("Player 1 sphere is null, cannot deactivate rings");
        }
        
        // Deactivate Player 2's rings
        if (player2Sphere != null)
        {
            ConcentricRings player2Rings = player2Sphere.GetComponent<ConcentricRings>();
            if (player2Rings != null && player2Rings.rings != null)
            {
                int deactivatedCount = 0;
                foreach (GameObject ring in player2Rings.rings)
                {
                    if (ring != null)
                    {
                        ring.SetActive(false);
                        deactivatedCount++;
                    }
                }
                Debug.Log("Deactivated " + deactivatedCount + " rings for Player 2");
                
                // Also disable the ConcentricRings component to prevent it from reactivating the rings
                player2Rings.enabled = false;
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 2 sphere");
            }
            
            // Also disable the Player2RingGameController to prevent it from interfering with multiplayer mode
            if (player2Controller != null)
            {
                player2Controller.enabled = false;
                // Don't deactivate the gameObject as it's the player sphere itself
                Debug.Log("Disabled Player2RingGameController on Player 2");
            }
        }
        else
        {
            Debug.LogWarning("Player 2 sphere is null, cannot deactivate rings");
        }
    }
    
    void ReactivateOriginalRings()
    {
        Debug.Log("Reactivating original rings...");
        
        // Reactivate Player 1's rings
        if (player1Sphere != null)
        {
            ConcentricRings player1Rings = player1Sphere.GetComponent<ConcentricRings>();
            if (player1Rings != null)
            {
                player1Rings.enabled = true;
                if (player1Rings.rings != null)
                {
                    foreach (GameObject ring in player1Rings.rings)
                    {
                        if (ring != null)
                        {
                            ring.SetActive(true);
                        }
                    }
                }
            }
            
            // Re-enable the RingGameController
            if (player1Controller != null)
            {
                player1Controller.enabled = true;
            }
        }
        
        // Reactivate Player 2's rings
        if (player2Sphere != null)
        {
            ConcentricRings player2Rings = player2Sphere.GetComponent<ConcentricRings>();
            if (player2Rings != null)
            {
                player2Rings.enabled = true;
                if (player2Rings.rings != null)
                {
                    foreach (GameObject ring in player2Rings.rings)
                    {
                        if (ring != null)
                        {
                            ring.SetActive(true);
                        }
                    }
                }
            }
            
            // Re-enable the Player2RingGameController
            if (player2Controller != null)
            {
                player2Controller.enabled = true;
            }
        }
    }
    
    void DeactivateMultiplayerMode()
    {
        multiplayerModeActive = false;
        Debug.Log("Multiplayer mode deactivated!");
        
        // Hide the multiplayer rings
        if (multiplayerRingObject != null)
        {
            multiplayerRingObject.SetActive(false);
        }
        
        // Also hide any individual expanding circles
        HideIndividualExpandingCircles();
        
        // Reactivate original rings
        ReactivateOriginalRings();
    }
    
    void StartPlayer1Expanding()
    {
        player1IsExpanding = true;
        player1CurrentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle at Player 1's position
        player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        
        // Set initial scale for the sphere
        player1ExpandingCircle.transform.localScale = new Vector3(player1CurrentRadius, player1CurrentRadius, player1CurrentRadius);
        
        // Set the expanding circle material to Player 1's color (yellow tint)
        player1ExpandingCircleMaterial.color = new Color(1f, 0.8f, 0.2f, 0.5f);
        
        // Activate the expanding circle
        player1ExpandingCircle.SetActive(true);
        
        Debug.Log("Started Player 1's expanding circle");
    }
    
    void StartPlayer2Expanding()
    {
        player2IsExpanding = true;
        player2CurrentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle at Player 2's position
        player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        
        // Set initial scale for the sphere
        player2ExpandingCircle.transform.localScale = new Vector3(player2CurrentRadius, player2CurrentRadius, player2CurrentRadius);
        
        // Set the expanding circle material to Player 2's color (blue tint)
        player2ExpandingCircleMaterial.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        
        // Activate the expanding circle
        player2ExpandingCircle.SetActive(true);
        
        Debug.Log("Started Player 2's expanding circle");
    }
    
    void CheckPlayer1Hit()
    {
        player1IsExpanding = false;
        float distanceFromSphereToCircleEdge = player1CurrentRadius / 2;
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        float distanceFromTarget = Mathf.Abs(distanceFromSphereToCircleEdge - activeRingRadius);
        
        // Show visual feedback
        StartCoroutine(ShowHitFeedback(player1ExpandingCircle, player1ExpandingCircleMaterial, distanceFromTarget, player1CurrentRadius));
        
        // Check if hit was successful
        if (distanceFromTarget < 0.5f) // Using a fixed tolerance
        {
            player1Success = true;
            Debug.Log("Player 1 hit the target!");
            CheckBothPlayersSuccess();
        }
        else
        {
            player1Success = false;
            Debug.Log("Player 1 missed. Distance from target: " + distanceFromTarget);
        }
    }
    
    void CheckPlayer2Hit()
    {
        player2IsExpanding = false;
        float distanceFromSphereToCircleEdge = player2CurrentRadius / 2;
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        float distanceFromTarget = Mathf.Abs(distanceFromSphereToCircleEdge - activeRingRadius);
        
        // Show visual feedback
        StartCoroutine(ShowHitFeedback(player2ExpandingCircle, player2ExpandingCircleMaterial, distanceFromTarget, player2CurrentRadius));
        
        // Check if hit was successful
        if (distanceFromTarget < 0.5f) // Using a fixed tolerance
        {
            player2Success = true;
            Debug.Log("Player 2 hit the target!");
            CheckBothPlayersSuccess();
        }
        else
        {
            player2Success = false;
            Debug.Log("Player 2 missed. Distance from target: " + distanceFromTarget);
        }
    }

    private IEnumerator ShowHitFeedback(GameObject circle, Material material, float distanceFromTarget, float currentRadius)
    {
        // Keep the expanding circle visible for feedback
        Color feedbackColor;
        
        if (distanceFromTarget < 0.5f) // Using fixed tolerance
        {
            // Good hit - green
            feedbackColor = Color.green;
        }
        else if (distanceFromTarget < 1.0f) // Double tolerance for "close"
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
        material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0.5f);
        
        // Flash the expanding circle
        float duration = 0.5f;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0.1f, time / duration);
            
            material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, alpha);
            
            // Keep the circle at the same size during feedback
            circle.transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);
            
            yield return null;
        }
        
        // Hide the circle after feedback
        circle.SetActive(false);
    }

    void CheckBothPlayersSuccess()
    {
        if (player1Success && player2Success)
        {
            // Both players hit the target, move to next ring
            SetRingColor(ringOrder[currentRingIndex], Color.black);
            currentRingIndex++;
            
            if (currentRingIndex >= ringOrder.Length)
            {
                // Game completed!
                Debug.Log(successMessage);
                gameCompleted = true;
            }
            else
            {
                // Activate the next ring
                SetRingColor(ringOrder[currentRingIndex], multiplayerRingColor);
                Debug.Log("Both players succeeded! Moving to next ring: " + ringOrder[currentRingIndex]);
                
                // Reset success flags
                player1Success = false;
                player2Success = false;
            }
        }
    }
    
    void SetRingColor(int ringIndex, Color color)
    {
        if (ringIndex >= 0 && ringIndex < rings.Length && rings[ringIndex] != null)
        {
            ringMaterials[ringIndex].color = color;
            
            // Also set emission for bright color
            if (color != Color.black)
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
    
    float GetRingRadius(int ringIndex)
    {
        float baseRadius = 2.0f;
        float ringSpacing = 1.0f;
        return baseRadius + (ringIndex * ringSpacing);
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
    
    // Public method to reset the game
    public void ResetGame()
    {
        // Deactivate multiplayer mode and reactivate original rings
        DeactivateMultiplayerMode();
        gameCompleted = false;

        // Reset player success tracking
        player1Success = false;
        player2Success = false;
        player1Distance = 0f;
        player2Distance = 0f;

        // Make sure the original rings are reactivated
        ReactivateOriginalRings();
    }
}