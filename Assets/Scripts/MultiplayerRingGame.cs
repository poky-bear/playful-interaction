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
    public GameObject multiplayerRingObject;
    
    // Private variables
    private float proximityTimer = 0f;
    private bool multiplayerModeActive = false;
    private bool gameCompleted = false;
    private GameObject expandingCircle;
    private Material expandingCircleMaterial;
    private float currentRadius = 0f;
    private bool isExpanding = false;
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
        
        // Create multiplayer ring object if not assigned
        if (multiplayerRingObject == null)
        {
            CreateMultiplayerRing();
        }
        
        // Create expanding circle for multiplayer mode
        CreateExpandingCircle();
    }
    
    void CreateMultiplayerRing()
    {
        // Create a parent object for the multiplayer rings
        multiplayerRingObject = new GameObject("MultiplayerRings");
        
        // Create rings
        rings = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            rings[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rings[i].name = "MultiplayerRing" + i;
            rings[i].transform.parent = multiplayerRingObject.transform;
            
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
    
    void CreateExpandingCircle()
    {
        // Create a sphere for the expanding circle
        expandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        expandingCircle.name = "MultiplayerExpandingCircle";
        
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
        
        // Set a semi-transparent color
        expandingCircleMaterial.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Apply the material to the sphere
        expandingCircle.GetComponent<Renderer>().material = expandingCircleMaterial;
        
        // Hide it initially
        expandingCircle.SetActive(false);
    }
    
    void Update()
    {
        if (player1Sphere == null || player2Sphere == null)
            return;
            
        if (gameCompleted)
            return;
            
        // Check if multiplayer mode is already active
        if (multiplayerModeActive)
        {
            UpdateMultiplayerGame();
        }
        else
        {
            // Check distance between players
            float distance = Vector3.Distance(player1Sphere.transform.position, player2Sphere.transform.position);
            
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
    
    void ActivateMultiplayerMode()
    {
        multiplayerModeActive = true;
        Debug.Log("Multiplayer mode activated!");
        
        // Position the multiplayer rings between the two players
        Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        multiplayerRingObject.transform.position = midpoint;
        
        // Set up the rings
        float baseRadius = 2.0f; // Base radius for the first ring
        float ringSpacing = 1.0f; // Spacing between rings
        
        for (int i = 0; i < 3; i++)
        {
            float radius = baseRadius + (i * ringSpacing);
            rings[i].transform.localScale = new Vector3(radius * 2, 0.1f, radius * 2); // Make rings thin
            rings[i].transform.position = midpoint;
            
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
        
        // Deactivate the original rings around both spheres
        DeactivateOriginalRings();
        
        // Reset player states
        player1Ready = false;
        player2Ready = false;
        player1Success = false;
        player2Success = false;
        player1Distance = 0f;
        player2Distance = 0f;
    }
    
    // Deactivate the original rings around both spheres
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
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 1 sphere");
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
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 2 sphere");
            }
        }
        else
        {
            Debug.LogWarning("Player 2 sphere is null, cannot deactivate rings");
        }
    }
    
    void UpdateMultiplayerGame()
    {
        // Check for player inputs
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player1Ready = true;
            // Start expanding circle immediately when spacebar is pressed in multiplayer mode
            if (!isExpanding)
            {
                StartExpanding();
            }
            else
            {
                CheckHit();
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player1Ready = false;
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            player2Ready = true;
            // Start expanding circle immediately when F key is pressed in multiplayer mode
            if (!isExpanding)
            {
                StartExpanding();
            }
            else
            {
                CheckHit();
            }
        }
        
        if (Input.GetKeyUp(KeyCode.F))
        {
            player2Ready = false;
        }
        
        // Update expanding circle if active
        if (isExpanding)
        {
            // Increase the radius based on time
            currentRadius += 1.0f * Time.deltaTime;
            
            // Update the scale of the expanding circle
            expandingCircle.transform.localScale = new Vector3(currentRadius, currentRadius, currentRadius);
            
            // Position at the midpoint between players
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            expandingCircle.transform.position = midpoint;
            multiplayerRingObject.transform.position = midpoint;
        }
        
        // Always keep the rings centered between the players
        if (multiplayerRingObject.activeSelf)
        {
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            multiplayerRingObject.transform.position = midpoint;
            
            // Check if players have moved too far apart
            float distance = Vector3.Distance(player1Sphere.transform.position, player2Sphere.transform.position);
            if (distance > activationDistance * 1.5f)
            {
                // Players moved too far apart, deactivate multiplayer mode
                DeactivateMultiplayerMode();
            }
        }
    }
    
    void CheckBothPlayersReady()
    {
        // This method is now only used for synchronization purposes
        // The expanding circle now starts when either player presses their key
        if (player1Ready && player2Ready)
        {
            // Both players are pressing their action buttons
            // This could be used for special actions in the future
            Debug.Log("Both players are ready simultaneously!");
        }
    }
    
    void StartExpanding()
    {
        isExpanding = true;
        currentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle at the midpoint between players
        Vector3 player1Pos = player1Sphere.transform.position;
        Vector3 player2Pos = player2Sphere.transform.position;
        Vector3 midpoint = (player1Pos + player2Pos) / 2f;
        expandingCircle.transform.position = midpoint;
        
        // Set the expanding circle material to dark color
        expandingCircleMaterial.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Activate the expanding circle
        expandingCircle.SetActive(true);
        
        // Log detailed information about the expanding circle
        Debug.Log("Started multiplayer expanding circle at midpoint: " + midpoint);
        Debug.Log("Player 1 position: " + player1Pos + ", Player 2 position: " + player2Pos);
        Debug.Log("Distance between players: " + Vector3.Distance(player1Pos, player2Pos));
        
        // Log which player initiated the expansion
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Expansion initiated by Player 1 (Space key)");
        }
        else if (Input.GetKey(KeyCode.F))
        {
            Debug.Log("Expansion initiated by Player 2 (F key)");
        }
    }
    
    void CheckHit()
    {
        isExpanding = false;
        
        // Get the current active ring radius
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        
        // Calculate the distance from the midpoint to the expanding circle edge
        Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        float distanceToCircleEdge = currentRadius / 2;
        
        // Calculate how close the players were to the target
        float distanceFromTarget = Mathf.Abs(distanceToCircleEdge - activeRingRadius);
        
        Debug.Log("Multiplayer - Radius of target ring: " + activeRingRadius + 
                ", Radius of user ring: " + distanceToCircleEdge + 
                ", total diff: " + distanceFromTarget);
        
        // Show visual feedback
        StartCoroutine(ShowHitFeedback(distanceFromTarget));
        
        // Check if the expanding circle is close to the active ring
        float hitTolerance = 0.5f; // Tolerance for hitting the ring
        
        // Determine if this attempt was successful
        bool currentAttemptSuccessful = (distanceFromTarget < hitTolerance);
        
        // Track which player triggered this check
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player1Success = currentAttemptSuccessful;
            player1Distance = distanceFromTarget;
            player1Ready = false;
            
            Debug.Log("Player 1 hit result: " + (player1Success ? "Success" : "Fail") + 
                      " (distance: " + player1Distance.ToString("F2") + ")");
        }
        else if (Input.GetKeyUp(KeyCode.F))
        {
            player2Success = currentAttemptSuccessful;
            player2Distance = distanceFromTarget;
            player2Ready = false;
            
            Debug.Log("Player 2 hit result: " + (player2Success ? "Success" : "Fail") + 
                      " (distance: " + player2Distance.ToString("F2") + ")");
        }
        
        // Check if both players have attempted the current ring
        if (player1Distance > 0 && player2Distance > 0)
        {
            // Both players have attempted, check if both were successful
            if (player1Success && player2Success)
            {
                // Both players succeeded! Move to the next ring
                SetRingColor(ringOrder[currentRingIndex], Color.black);
                currentRingIndex++;
                
                if (currentRingIndex >= ringOrder.Length)
                {
                    // Game completed!
                    Debug.Log("Multiplayer: " + successMessage);
                    gameCompleted = true;
                    
                    // Hide the expanding circle
                    expandingCircle.SetActive(false);
                    currentRadius = 0f;
                    
                    // Hide the multiplayer rings
                    multiplayerRingObject.SetActive(false);
                    
                    // Keep the original rings deactivated
                    // (We don't call ReactivateOriginalRings() here)
                    
                    // Show completion message
                    MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                    if (ui != null)
                    {
                        ui.ShowGameCompleteMessage();
                    }
                }
                else
                {
                    // Activate the next ring in the order
                    SetRingColor(ringOrder[currentRingIndex], multiplayerRingColor);
                    Debug.Log("Multiplayer good hit by both players! Moving to next ring: " + ringOrder[currentRingIndex]);
                    
                    // Reset the expanding circle for the next attempt
                    expandingCircle.SetActive(false);
                    currentRadius = 0f;
                    
                    // Reset player success tracking for the next ring
                    player1Success = false;
                    player2Success = false;
                    player1Distance = 0f;
                    player2Distance = 0f;
                    
                    // Update UI with success message
                    MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                    if (ui != null)
                    {
                        ui.ShowHitFeedback("Great teamwork! Both players hit the target!", Color.green);
                    }
                }
            }
            else
            {
                // At least one player failed
                Debug.Log("Multiplayer: Player 1 " + (player1Success ? "succeeded" : "failed") + 
                          " and Player 2 " + (player2Success ? "succeeded" : "failed") + 
                          ". Both must succeed to advance.");
                
                // Provide feedback on what happened
                MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                if (ui != null)
                {
                    if (!player1Success && !player2Success)
                    {
                        ui.ShowHitFeedback("Both players missed. Try again together!", Color.red);
                    }
                    else if (!player1Success)
                    {
                        ui.ShowHitFeedback("Player 1 missed. Try again together!", new Color(1f, 0.6f, 0f));
                    }
                    else if (!player2Success)
                    {
                        ui.ShowHitFeedback("Player 2 missed. Try again together!", new Color(1f, 0.6f, 0f));
                    }
                }
                
                // Reset for another attempt
                expandingCircle.SetActive(false);
                currentRadius = 0f;
                
                // Reset player success tracking for another attempt at the same ring
                player1Success = false;
                player2Success = false;
                player1Distance = 0f;
                player2Distance = 0f;
            }
        }
        else
        {
            // Only one player has attempted so far
            // Provide feedback on how close they were
            if (currentAttemptSuccessful)
            {
                Debug.Log("Multiplayer: Good hit! Waiting for the other player...");
                
                // Update UI with waiting message
                MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                if (ui != null)
                {
                    if (player1Success)
                    {
                        ui.ShowHitFeedback("Player 1 hit the target! Waiting for Player 2...", Color.green);
                    }
                    else if (player2Success)
                    {
                        ui.ShowHitFeedback("Player 2 hit the target! Waiting for Player 1...", Color.green);
                    }
                }
            }
            else
            {
                if (distanceFromTarget < hitTolerance * 2)
                {
                    Debug.Log("Multiplayer close! You were " + distanceFromTarget.ToString("F2") + " units away.");
                    
                    // Update UI with close message
                    MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                    if (ui != null)
                    {
                        ui.ShowHitFeedback("Close! Try again together.", new Color(1f, 0.6f, 0f));
                    }
                }
                else
                {
                    Debug.Log("Multiplayer miss! You were " + distanceFromTarget.ToString("F2") + " units away.");
                    
                    // Update UI with miss message
                    MultiplayerRingGameUI ui = FindObjectOfType<MultiplayerRingGameUI>();
                    if (ui != null)
                    {
                        ui.ShowHitFeedback("Miss! Try again together.", Color.red);
                    }
                }
                
                // Reset the expanding circle for another attempt
                expandingCircle.SetActive(false);
                currentRadius = 0f;
                
                // Reset player success tracking for another attempt
                player1Success = false;
                player2Success = false;
                player1Distance = 0f;
                player2Distance = 0f;
            }
        }
    }
    
    private IEnumerator ShowHitFeedback(float distanceFromTarget)
    {
        // Keep the expanding circle visible for feedback
        Color feedbackColor;
        float hitTolerance = 0.5f;
        
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
            
            // Ensure the circle stays centered between the players
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            expandingCircle.transform.position = midpoint;
            
            yield return null;
        }
        
        // Only hide the expanding circle if we're not moving to the next ring
        if (distanceFromTarget >= hitTolerance)
        {
            // Hide the expanding circle
            expandingCircle.SetActive(false);
            
            // Reset the expanding circle for the next attempt
            currentRadius = 0f;
        }
        
        // Reset the expanding circle material color back to dark
        expandingCircleMaterial.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
    }
    
    void DeactivateMultiplayerMode()
    {
        multiplayerModeActive = false;
        proximityTimer = 0f;
        
        // Hide the multiplayer rings
        multiplayerRingObject.SetActive(false);
        
        // Hide the expanding circle
        expandingCircle.SetActive(false);
        currentRadius = 0f;
        isExpanding = false;
        
        // Reset player states
        player1Ready = false;
        player2Ready = false;
        player1Success = false;
        player2Success = false;
        player1Distance = 0f;
        player2Distance = 0f;
        
        // Reactivate the original rings around both spheres
        ReactivateOriginalRings();
        
        Debug.Log("Multiplayer mode deactivated!");
    }
    
    // Reactivate the original rings around both spheres
    void ReactivateOriginalRings()
    {
        Debug.Log("Reactivating original rings after multiplayer mode...");
        
        // Reactivate Player 1's rings
        if (player1Sphere != null)
        {
            ConcentricRings player1Rings = player1Sphere.GetComponent<ConcentricRings>();
            if (player1Rings != null && player1Rings.rings != null)
            {
                int reactivatedCount = 0;
                foreach (GameObject ring in player1Rings.rings)
                {
                    if (ring != null)
                    {
                        ring.SetActive(true);
                        reactivatedCount++;
                    }
                }
                Debug.Log("Reactivated " + reactivatedCount + " rings for Player 1");
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 1 sphere");
            }
        }
        else
        {
            Debug.LogWarning("Player 1 sphere is null, cannot reactivate rings");
        }
        
        // Reactivate Player 2's rings
        if (player2Sphere != null)
        {
            ConcentricRings player2Rings = player2Sphere.GetComponent<ConcentricRings>();
            if (player2Rings != null && player2Rings.rings != null)
            {
                int reactivatedCount = 0;
                foreach (GameObject ring in player2Rings.rings)
                {
                    if (ring != null)
                    {
                        ring.SetActive(true);
                        reactivatedCount++;
                    }
                }
                Debug.Log("Reactivated " + reactivatedCount + " rings for Player 2");
            }
            else
            {
                Debug.LogWarning("Could not find ConcentricRings component or rings array on Player 2 sphere");
            }
        }
        else
        {
            Debug.LogWarning("Player 2 sphere is null, cannot reactivate rings");
        }
    }
    
    void SetRingColor(int ringIndex, Color color)
    {
        if (ringIndex >= 0 && ringIndex < rings.Length && rings[ringIndex] != null)
        {
            ringMaterials[ringIndex].color = color;
            
            // Also set emission for bright color
            if (color == multiplayerRingColor)
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
        // Calculate ring radius based on the ring's scale
        if (rings[ringIndex] != null)
        {
            return rings[ringIndex].transform.localScale.x / 2f;
        }
        
        return 0f;
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