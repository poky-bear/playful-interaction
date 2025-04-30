using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerRingGame : MonoBehaviour
{
    [Header("Multiplayer Settings")]
    [Tooltip("Distance at which multiplayer mode activates")]
    public float activationDistance = 3.0f;
    
    [Header("Colors")]
    [Tooltip("Dark color for inactive rings")]
    public Color darkColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    
    [Tooltip("Bright color for active ring")]
    public Color brightColor = new Color(0.8f, 0.2f, 0.8f, 1f); // Purple for active ring
    
    [Header("Ring Settings")]
    [Tooltip("Minimum distance between sphere and first ring")]
    public float minDistanceToFirstRing = 1.0f;
    
    [Tooltip("Base radius for the first ring")]
    public float baseRingRadius = 2.0f;
    
    [Tooltip("Height at which rings should stay")]
    public float ringHeight = 0.0f;
    
    [Tooltip("How smoothly rings follow player movement (higher = smoother)")]
    public float smoothSpeed = 5.0f;
    
    [Tooltip("Tolerance for hitting the active ring (smaller = more precise)")]
    public float hitTolerance = 0.0001f;
    
    [Tooltip("Spacing between consecutive rings")]
    public float ringSpacing = 1.5f;
    
    [Tooltip("Thickness of the rings")]
    public float ringThickness = 0.2f;
    
    [Tooltip("Speed at which the circles expand")]
    public float expansionSpeed = 1.0f;
    
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
    
    [Tooltip("Reference to the game mode transition manager")]
    public GameModeTransitionManager transitionManager;
    
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
    private bool player1Hit = false;
    private bool player2Hit = false;
    private bool isExpanding = false;
    private float currentRadius = 0f;
    private GameObject centerExpandingCircle;

    private void SetupBoidTargets()
    {
        BoidManager boidManager = FindObjectOfType<BoidManager>();
        if (boidManager != null)
        {
            boidManager.SetMultiplayerMode(true, player1Sphere.transform, player2Sphere.transform);
            Debug.Log("Boids now following both players in multiplayer mode");
        }
    }
    
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
            midpoint.y = ringHeight; // Force rings to stay at fixed height
            multiplayerRingObject.transform.position = midpoint;
        }
        
        // Create rings
        rings = new GameObject[3];
        
        for (int i = 0; i < 3; i++)
        {
            // Calculate the radius for this ring using the configurable settings
            float radius = minDistanceToFirstRing + baseRingRadius + (i * ringSpacing);
            
            // Create a ring using a torus mesh
            rings[i] = new GameObject("MultiplayerRing" + i);
            rings[i].transform.parent = multiplayerRingObject.transform;
            rings[i].transform.localPosition = Vector3.zero;
            
            // Add mesh components
            MeshFilter meshFilter = rings[i].AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = rings[i].AddComponent<MeshRenderer>();
            
            // Generate torus mesh
            meshFilter.mesh = CreateTorusMesh(radius, ringThickness * 0.5f); // Half thickness for better proportions
            
            // Add collider for interaction (as trigger)
            MeshCollider meshCollider = rings[i].AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshCollider.convex = true; // Required for triggers
            meshCollider.isTrigger = true; // Make it a trigger collider so it doesn't block movement
            
            // Create material for the ring with transparency
            Material ringMaterial = new Material(Shader.Find("Standard"));
            ringMaterial.SetFloat("_Mode", 3); // Transparent mode
            ringMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ringMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ringMaterial.SetInt("_ZWrite", 0);
            ringMaterial.DisableKeyword("_ALPHATEST_ON");
            ringMaterial.EnableKeyword("_ALPHABLEND_ON");
            ringMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            ringMaterial.renderQueue = 3000;
            
            // Set initial color to a semi-transparent dark color
            Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            ringMaterial.color = inactiveColor;
            
            // Enable emission for better visibility
            ringMaterial.EnableKeyword("_EMISSION");
            ringMaterial.SetColor("_EmissionColor", inactiveColor * 0.3f);
            
            // Store the material
            originalMaterials[i] = ringMaterial;
            ringMaterials[i] = new Material(ringMaterial);
            
            // Apply the material to the ring
            rings[i].GetComponent<Renderer>().material = ringMaterials[i];
        }
        
        // Show the rings
        multiplayerRingObject.SetActive(true);
    }
    
    void CreateExpandingCircles()
    {
        // Create the center expanding circle
        centerExpandingCircle = new GameObject("CenterExpandingCircle");
        centerExpandingCircle.transform.parent = null; // Ensure it's at root level
        
        // Add mesh components
        MeshFilter meshFilter = centerExpandingCircle.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = centerExpandingCircle.AddComponent<MeshRenderer>();
        
        // Create material for the expanding circle
        Material material = new Material(Shader.Find("Standard"));
        SetupTransparentMaterial(material);
        material.color = new Color(0.8f, 0.2f, 0.8f, 0.5f); // Purple tint to match multiplayer color
        meshRenderer.material = material;
        
        // Initialize with zero radius
        UpdateExpandingCircleMesh(meshFilter, 0f);
        centerExpandingCircle.SetActive(false);
    }
    
    void UpdateExpandingCircleMesh(MeshFilter meshFilter, float radius)
    {
        // Create a ring mesh with the current radius
        meshFilter.mesh = CreateTorusMesh(radius, ringThickness);
    }

    void UpdateRingParameters()
    {
        if (rings == null) return;

        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
            {
                float radius = minDistanceToFirstRing + baseRingRadius + (i * ringSpacing);
                MeshFilter meshFilter = rings[i].GetComponent<MeshFilter>();
                if (meshFilter != null)
                {
                    meshFilter.mesh = CreateTorusMesh(radius, ringThickness * 0.5f);
                    
                    // Update collider as well
                    MeshCollider meshCollider = rings[i].GetComponent<MeshCollider>();
                    if (meshCollider != null)
                    {
                        meshCollider.sharedMesh = meshFilter.mesh;
                    }
                }
            }
        }
    }

    void OnValidate()
    {
        // Update ring parameters whenever they are changed in the inspector
        UpdateRingParameters();
    }

    void StartExpanding()
    {
        isExpanding = true;
        currentRadius = 0.1f; // Start with a small radius
        
        // Calculate the midpoint between players
        Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        midpoint.y = ringHeight; // Keep at same height as rings
        
        // Position and show the expanding circle
        centerExpandingCircle.transform.position = midpoint;
        UpdateExpandingCircleMesh(centerExpandingCircle.GetComponent<MeshFilter>(), currentRadius);
        centerExpandingCircle.SetActive(true);
        
        // Reset hit flags
        player1Hit = false;
        player2Hit = false;
    }
    
    void CheckHit(bool isPlayer1)
    {
        // Get the current active ring radius
        float activeRingRadius = GetRingRadius(currentRingIndex);
        
        // Calculate the distance from the center to the expanding circle edge
        float distanceToEdge = currentRadius;
        
        // Calculate how close the expanding circle was to the target
        float distanceFromTarget = Mathf.Abs(distanceToEdge - activeRingRadius);
        
        // Check if the hit was within tolerance
        if (distanceFromTarget < hitTolerance)
        {
            // Mark the player's hit
            if (isPlayer1)
            {
                player1Hit = true;
                Debug.Log("Player 1 hit the ring!");
            }
            else
            {
                player2Hit = true;
                Debug.Log("Player 2 hit the ring!");
            }
            
            // If both players have hit the ring, move to the next one
            if (player1Hit && player2Hit)
            {
                // Success! Move to the next ring
                SetRingColor(currentRingIndex, darkColor);
                currentRingIndex++;
                
                if (currentRingIndex >= rings.Length)
                {
                    // Game completed!
                    Debug.Log(successMessage);
                    gameCompleted = true;
                    centerExpandingCircle.SetActive(false);
                }
                else
                {
                    // Activate the next ring
                    SetRingColor(currentRingIndex, brightColor);
                    Debug.Log("Both players hit! Moving to next ring: " + currentRingIndex);
                }
                
                // Reset for next ring
                isExpanding = false;
                currentRadius = 0f;
                centerExpandingCircle.SetActive(false);
                player1Hit = false;
                player2Hit = false;
            }
        }
        else
        {
            // Miss - stop expanding but keep the circle visible for feedback
            isExpanding = false;
            StartCoroutine(ShowHitFeedback(distanceFromTarget));
        }
    }
    
    private IEnumerator ShowHitFeedback(float distanceFromTarget)
    {
        // Get the renderer and material
        MeshRenderer renderer = centerExpandingCircle.GetComponent<MeshRenderer>();
        Material material = renderer.material;
        
        // Set feedback color based on how close it was
        Color feedbackColor;
        if (distanceFromTarget < hitTolerance * 2)
        {
            feedbackColor = new Color(1f, 0.6f, 0f); // Orange for close
        }
        else
        {
            feedbackColor = Color.red; // Red for miss
        }
        
        // Flash the feedback color
        float duration = 0.5f;
        float time = 0;
        
        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0.5f, 0.1f, time / duration);
            material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, alpha);
            yield return null;
        }
        
        // Hide the circle and reset
        centerExpandingCircle.SetActive(false);
        currentRadius = 0f;
        material.color = new Color(0.8f, 0.2f, 0.8f, 0.5f); // Reset to original color
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

    Mesh CreateTorusMesh(float radius, float tubeRadius)
    {
        Mesh mesh = new Mesh();
        
        int tubularSegments = 32;
        int radialSegments = 16;
        
        int numVertices = (tubularSegments + 1) * (radialSegments + 1);
        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];
        int[] triangles = new int[tubularSegments * radialSegments * 6];
        
        // Generate vertices
        for (int i = 0; i <= tubularSegments; i++)
        {
            float u = (float)i / tubularSegments * 2f * Mathf.PI;
            
            for (int j = 0; j <= radialSegments; j++)
            {
                float v = (float)j / radialSegments * 2f * Mathf.PI;
                
                float x = (radius + tubeRadius * Mathf.Cos(v)) * Mathf.Cos(u);
                float y = (radius + tubeRadius * Mathf.Cos(v)) * Mathf.Sin(u);
                float z = tubeRadius * Mathf.Sin(v);
                
                int vertIndex = i * (radialSegments + 1) + j;
                
                vertices[vertIndex] = new Vector3(x, z, y); // Adjust for Unity's coordinate system
                uv[vertIndex] = new Vector2((float)i / tubularSegments, (float)j / radialSegments);
            }
        }
        
        // Generate triangles
        int index = 0;
        for (int i = 0; i < tubularSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * (radialSegments + 1) + j;
                int b = i * (radialSegments + 1) + j + 1;
                int c = (i + 1) * (radialSegments + 1) + j + 1;
                int d = (i + 1) * (radialSegments + 1) + j;
                
                // First triangle
                triangles[index++] = a;
                triangles[index++] = b;
                triangles[index++] = d;
                
                // Second triangle
                triangles[index++] = b;
                triangles[index++] = c;
                triangles[index++] = d;
            }
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
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
            // Calculate the midpoint between players
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            midpoint.y = ringHeight; // Force rings to stay at fixed height
            
            // Update the position of the multiplayer rings to stay centered with smooth movement
            if (multiplayerRingObject != null)
            {
                Vector3 currentPos = multiplayerRingObject.transform.position;
                Vector3 targetPos = Vector3.Lerp(currentPos, midpoint, Time.deltaTime * smoothSpeed);
                multiplayerRingObject.transform.position = targetPos;
            }
            
            // Check if players have moved too far apart
            if (distance > activationDistance * 1.5f)
            {
                // Players moved too far apart, deactivate multiplayer mode
                DeactivateMultiplayerMode();
                Debug.Log("Players moved too far apart. Distance: " + distance + ", Deactivating multiplayer mode.");
            }
            else
            {
                // Handle player inputs
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    player1Ready = true;
                    if (!isExpanding)
                    {
                        StartExpanding();
                    }
                }
                if (Input.GetKeyDown(KeyCode.F))
                {
                    player2Ready = true;
                    if (!isExpanding)
                    {
                        StartExpanding();
                    }
                }
                
                // Update expanding circle
                if (isExpanding)
                {
                    // Calculate the center point between players
                    Vector3 centerPoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
                    centerPoint.y = ringHeight; // Keep at same height as rings
                    
                    // Update radius and position
                    currentRadius += expansionSpeed * Time.deltaTime;
                    centerExpandingCircle.transform.position = centerPoint;
                    UpdateExpandingCircleMesh(centerExpandingCircle.GetComponent<MeshFilter>(), currentRadius);
                    
                    // Check for key releases
                    if (Input.GetKeyUp(KeyCode.Space))
                    {
                        CheckHit(true);  // Player 1
                    }
                    if (Input.GetKeyUp(KeyCode.F))
                    {
                        CheckHit(false); // Player 2
                    }
                }
                
                // Continue with other multiplayer game updates
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
        // Update expanding circles
        if (player1IsExpanding)
        {
            player1CurrentRadius += expansionSpeed * Time.deltaTime;
            UpdateExpandingCircleMesh(player1ExpandingCircle.GetComponent<MeshFilter>(), player1CurrentRadius);
            player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        }
        
        if (player2IsExpanding)
        {
            player2CurrentRadius += expansionSpeed * Time.deltaTime;
            UpdateExpandingCircleMesh(player2ExpandingCircle.GetComponent<MeshFilter>(), player2CurrentRadius);
            player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        }
        
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
            // Calculate the midpoint between players
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;

            // Update Player 1's expanding circle
            if (player1IsExpanding)
            {
                player1CurrentRadius += 1.0f * Time.deltaTime;
                player1ExpandingCircle.transform.localScale = new Vector3(player1CurrentRadius, player1CurrentRadius, player1CurrentRadius);
                player1ExpandingCircle.transform.position = midpoint;
            }
            
            // Update Player 2's expanding circle
            if (player2IsExpanding)
            {
                player2CurrentRadius += 1.0f * Time.deltaTime;
                player2ExpandingCircle.transform.localScale = new Vector3(player2CurrentRadius, player2CurrentRadius, player2CurrentRadius);
                player2ExpandingCircle.transform.position = midpoint;
            }
            
            // Position updates are handled in the main Update function
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
        
        // Create the rings if they don't exist
        if (rings == null || rings[0] == null)
        {
            CreateMultiplayerRing();
        }

        // Set up the rings with increasing sizes        
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
            {
                // No need to scale the rings as they are created with the correct size
                
                // Set all rings to dark initially
                SetRingColor(i, new Color(0.1f, 0.1f, 0.1f, 1f)); // Darker black for better contrast
            }
            else
            {
                Debug.LogError("Ring " + i + " is null in ActivateMultiplayerMode!");
            }
        }
        
        // Generate random order for the rings
        ringOrder = GenerateRandomOrder();
        currentRingIndex = 0;
        
        // Set the first ring to the multiplayer color with glow
        if (ringOrder != null && currentRingIndex < ringOrder.Length)
        {
            SetRingColor(ringOrder[currentRingIndex], multiplayerRingColor);
        }
        else
        {
            Debug.LogError("Invalid ring order or current index in ActivateMultiplayerMode!");
        }
        
        // Show the multiplayer rings
        multiplayerRingObject.SetActive(true);
        
        // Reset player states
        player1Ready = false;
        player2Ready = false;
        player1Success = false;
        player2Success = false;
        player1Distance = 0f;
        player2Distance = 0f;
        
        // Log the ring order for debugging
        string ringOrderStr = "Ring order: ";
        for (int i = 0; i < ringOrder.Length; i++)
        {
            ringOrderStr += ringOrder[i].ToString() + (i < ringOrder.Length - 1 ? " -> " : "");
        }
        Debug.Log(ringOrderStr);
        Debug.Log("Current active ring: " + ringOrder[currentRingIndex] + " (radius: " + GetRingRadius(ringOrder[currentRingIndex]) + ")");
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
            if (player1Rings != null)
            {
                // Disable the component itself
                player1Rings.enabled = false;
                
                if (player1Rings.rings != null)
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
            }
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
        
        // Reset boid targets to single player mode if game is completed
        if (gameCompleted)
        {
            BoidManager boidManager = FindObjectOfType<BoidManager>();
            if (boidManager != null)
            {
                boidManager.SetMultiplayerMode(false);
                Debug.Log("Boids returning to single player mode");
            }
        }
        
        // Reactivate original rings
        ReactivateOriginalRings();
    }
    
    void StartPlayer1Expanding()
    {
        player1IsExpanding = true;
        player1CurrentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle based on game mode
        if (multiplayerModeActive)
        {
            // In multiplayer mode, start from the midpoint
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            player1ExpandingCircle.transform.position = midpoint;
        }
        else
        {
            // In single player mode, start from player's position
            player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        }
        
        // Set initial scale for the sphere
        player1ExpandingCircle.transform.localScale = new Vector3(player1CurrentRadius, player1CurrentRadius, player1CurrentRadius);
        
        // Set the expanding circle material to Player 1's color (yellow tint)
        player1ExpandingCircleMaterial.color = new Color(1f, 0.8f, 0.2f, 0.5f);
        
        // Activate the expanding circle
        player1ExpandingCircle.SetActive(true);
        
        Debug.Log("Started Player 1's expanding circle" + (multiplayerModeActive ? " in multiplayer mode" : ""));
    }
    
    void StartPlayer2Expanding()
    {
        player2IsExpanding = true;
        player2CurrentRadius = 0.1f; // Start with a small radius
        
        // Position the expanding circle based on game mode
        if (multiplayerModeActive)
        {
            // In multiplayer mode, start from the midpoint
            Vector3 midpoint = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
            player2ExpandingCircle.transform.position = midpoint;
        }
        else
        {
            // In single player mode, start from player's position
            player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        }
        
        // Set initial scale for the sphere
        player2ExpandingCircle.transform.localScale = new Vector3(player2CurrentRadius, player2CurrentRadius, player2CurrentRadius);
        
        // Set the expanding circle material to Player 2's color (blue tint)
        player2ExpandingCircleMaterial.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        
        // Activate the expanding circle
        player2ExpandingCircle.SetActive(true);
        
        Debug.Log("Started Player 2's expanding circle" + (multiplayerModeActive ? " in multiplayer mode" : ""));
    }
    
    void CheckPlayer1Hit()
    {
        player1IsExpanding = false;
        float distanceFromSphereToCircleEdge = (player1CurrentRadius / 2);
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        float distanceFromTarget = Mathf.Abs(distanceFromSphereToCircleEdge - activeRingRadius);
        float tolerance = 0.05f; // Tighter tolerance for more precise gameplay
        
        // Show visual feedback
        StartCoroutine(ShowHitFeedback(player1ExpandingCircle, player1ExpandingCircleMaterial, distanceFromTarget, player1CurrentRadius));
        
        // Check if hit was successful
        if (distanceFromTarget < tolerance)
        {
            player1Success = true;
            Debug.Log("Player 1 hit the target perfectly! Distance: " + distanceFromTarget.ToString("F2"));
            
            if (player2Success)
            {
                Debug.Log("Both players have hit the target!");
            }
            else
            {
                Debug.Log("Waiting for Player 2 to hit the target...");
            }
            
            CheckBothPlayersSuccess();
        }
        else if (distanceFromTarget < tolerance * 2)
        {
            player1Success = false;
            Debug.Log("Player 1 was close! Distance: " + distanceFromTarget.ToString("F2") + " (need " + tolerance.ToString("F2") + " or less)");
        }
        else
        {
            player1Success = false;
            Debug.Log("Player 1 missed. Distance: " + distanceFromTarget.ToString("F2") + " (need " + tolerance.ToString("F2") + " or less)");
        }
    }
    
    void CheckPlayer2Hit()
    {
        player2IsExpanding = false;
        // Scale the radius by the same factor as the sphere to match the ring radius calculation
    
        float distanceFromSphereToCircleEdge = (player2CurrentRadius / 2);
        float activeRingRadius = GetRingRadius(ringOrder[currentRingIndex]);
        float distanceFromTarget = Mathf.Abs(distanceFromSphereToCircleEdge - activeRingRadius);
        float tolerance = 0.3f; // Tighter tolerance for more precise gameplay
        
        // Show visual feedback
        StartCoroutine(ShowHitFeedback(player2ExpandingCircle, player2ExpandingCircleMaterial, distanceFromTarget, player2CurrentRadius));
        
        // Check if hit was successful
        if (distanceFromTarget < tolerance)
        {
            player2Success = true;
            Debug.Log("Player 2 hit the target perfectly! Distance: " + distanceFromTarget.ToString("F2"));
            
            if (player1Success)
            {
                Debug.Log("Both players have hit the target!");
            }
            else
            {
                Debug.Log("Waiting for Player 1 to hit the target...");
            }
            
            CheckBothPlayersSuccess();
        }
        else if (distanceFromTarget < tolerance * 2)
        {
            player2Success = false;
            Debug.Log("Player 2 was close! Distance: " + distanceFromTarget.ToString("F2") + " (need " + tolerance.ToString("F2") + " or less)");
        }
        else
        {
            player2Success = false;
            Debug.Log("Player 2 missed. Distance: " + distanceFromTarget.ToString("F2") + " (need " + tolerance.ToString("F2") + " or less)");
        }
    }

    private IEnumerator ShowHitFeedback(GameObject circle, Material material, float distanceFromTarget, float currentRadius)
    {
        // Keep the expanding circle visible for feedback
        Color feedbackColor;
        float tolerance = 0.3f;
        
        if (distanceFromTarget < tolerance)
        {
            // Perfect hit - bright green with glow
            feedbackColor = new Color(0f, 1f, 0.2f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", feedbackColor * 0.5f);
        }
        else if (distanceFromTarget < tolerance * 2)
        {
            // Close - orange with slight glow
            feedbackColor = new Color(1f, 0.6f, 0f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", feedbackColor * 0.3f);
        }
        else
        {
            // Miss - red, no glow
            feedbackColor = new Color(1f, 0.2f, 0.2f);
            material.DisableKeyword("_EMISSION");
        }
        
        // Initial flash with high opacity
        material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, 0.8f);
        
        // Pulse effect
        float pulseDuration = 0.75f;
        float time = 0;
        
        while (time < pulseDuration)
        {
            time += Time.deltaTime;
            
            // Create a pulsing effect
            float pulse = Mathf.Sin(time * 10f) * 0.2f + 0.8f; // Oscillate between 0.6 and 1.0
            float alpha = Mathf.Lerp(0.8f, 0.2f, time / pulseDuration) * pulse;
            
            material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, alpha);
            
            // Keep the circle at the same size but add slight pulsing
            float scalePulse = 1f + (pulse - 0.8f) * 0.1f;
            circle.transform.localScale = new Vector3(currentRadius * scalePulse, currentRadius * scalePulse, currentRadius * scalePulse);
            
            yield return null;
        }
        
        // Fade out quickly
        float fadeOutDuration = 0.25f;
        time = 0;
        
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0.2f, 0f, time / fadeOutDuration);
            
            material.color = new Color(feedbackColor.r, feedbackColor.g, feedbackColor.b, alpha);
            
            yield return null;
        }
        
        // Disable emission and hide the circle
        material.DisableKeyword("_EMISSION");
        circle.SetActive(false);
    }

    void CheckBothPlayersSuccess()
    {
        if (player1Success && player2Success)
        {
            // Both players hit the target, celebrate and move to next ring
            StartCoroutine(CelebrateBothPlayersSuccess());
        }
    }
    
    private IEnumerator CelebrateBothPlayersSuccess()
    {
        // Create a bright success color for the completed ring
        Color successColor = new Color(0f, 1f, 0.5f, 1f); // Bright green
        
        // Flash the completed ring
        float flashDuration = 1.0f;
        float time = 0;
        
        while (time < flashDuration)
        {
            time += Time.deltaTime;
            float pulse = Mathf.Sin(time * 15f) * 0.5f + 0.5f; // Faster pulse
            Color pulseColor = Color.Lerp(successColor, Color.white, pulse);
            
            // Make the completed ring flash
            ringMaterials[ringOrder[currentRingIndex]].color = pulseColor;
            ringMaterials[ringOrder[currentRingIndex]].EnableKeyword("_EMISSION");
            ringMaterials[ringOrder[currentRingIndex]].SetColor("_EmissionColor", pulseColor);
            
            yield return null;
        }
        
        // Set the completed ring to a dark color
        SetRingColor(ringOrder[currentRingIndex], new Color(0.1f, 0.1f, 0.1f, 1f));
        
        // Move to the next ring
        currentRingIndex++;
        
        if (currentRingIndex >= ringOrder.Length)
        {
            // Game completed!
            Debug.Log(successMessage);
            gameCompleted = true;
            
            // Final celebration
            StartCoroutine(CelebrateGameCompletion());
            
            // Start the transition to the next game mode
            if (transitionManager != null)
            {
                transitionManager.StartTransition();
                Debug.Log("Starting game mode transition after multiplayer game completion");
            }
            else
            {
                Debug.LogError("TransitionManager not found! Cannot transition to next game mode.");
            }
        }
        else
        {
            // Activate the next ring with a smooth transition
            StartCoroutine(ActivateNextRing());
        }
        
        // Reset success flags
        player1Success = false;
        player2Success = false;
    }
    
    private IEnumerator ActivateNextRing()
    {
        // Fade in the next active ring
        float fadeDuration = 0.5f;
        float time = 0;
        
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            
            // Lerp from black to the multiplayer color
            Color currentColor = Color.Lerp(Color.black, multiplayerRingColor, t);
            ringMaterials[ringOrder[currentRingIndex]].color = currentColor;
            
            // Gradually enable emission
            ringMaterials[ringOrder[currentRingIndex]].EnableKeyword("_EMISSION");
            ringMaterials[ringOrder[currentRingIndex]].SetColor("_EmissionColor", currentColor * t * 0.5f);
            
            yield return null;
        }
        
        // Ensure final color is set
        SetRingColor(ringOrder[currentRingIndex], multiplayerRingColor);
        Debug.Log("Next active ring: " + ringOrder[currentRingIndex] + " (radius: " + GetRingRadius(ringOrder[currentRingIndex]) + ")");
    }
    
    private IEnumerator CelebrateGameCompletion()
    {
        // Set up boid targets for multiplayer mode
        SetupBoidTargets();

        // Make all rings glow in celebration
        float celebrationDuration = 2.0f;
        float time = 0;
        
        while (time < celebrationDuration)
        {
            time += Time.deltaTime;
            
            // Create a rainbow effect
            for (int i = 0; i < rings.Length; i++)
            {
                float hue = (time * 0.5f + i * 0.33f) % 1f;
                Color celebrationColor = Color.HSVToRGB(hue, 0.8f, 1f);
                
                ringMaterials[i].color = celebrationColor;
                ringMaterials[i].EnableKeyword("_EMISSION");
                ringMaterials[i].SetColor("_EmissionColor", celebrationColor * 0.7f);
            }
            
            yield return null;
        }
        
        // Fade out all rings
        time = 0;
        float fadeOutDuration = 1.0f;
        
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float alpha = 1f - (time / fadeOutDuration);
            
            for (int i = 0; i < rings.Length; i++)
            {
                Color currentColor = ringMaterials[i].color;
                ringMaterials[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                ringMaterials[i].SetColor("_EmissionColor", currentColor * alpha * 0.7f);
            }
            
            yield return null;
        }
        
        // Hide the rings
        multiplayerRingObject.SetActive(false);
    }
    
    void SetRingColor(int ringIndex, Color color)
    {
        if (ringIndex >= 0 && ringIndex < rings.Length && rings[ringIndex] != null)
        {
            // Determine if this is the active ring (using the multiplayer color)
            bool isActiveRing = (color == multiplayerRingColor);
            
            if (isActiveRing)
            {
                // Make active ring more visible with higher opacity and stronger glow
                ringMaterials[ringIndex].color = new Color(color.r, color.g, color.b, 0.9f);
                ringMaterials[ringIndex].EnableKeyword("_EMISSION");
                ringMaterials[ringIndex].SetColor("_EmissionColor", color * 1.5f); // Stronger emission
                
                // Start a subtle pulsing effect for the active ring
                StartCoroutine(PulseActiveRing(ringIndex));
            }
            else if (color == Color.black || color == new Color(0.1f, 0.1f, 0.1f, 1f))
            {
                // Inactive rings are semi-transparent and darker
                ringMaterials[ringIndex].color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
                ringMaterials[ringIndex].EnableKeyword("_EMISSION");
                ringMaterials[ringIndex].SetColor("_EmissionColor", new Color(0.2f, 0.2f, 0.2f) * 0.3f);
            }
            else
            {
                // For other colors (like during celebrations)
                ringMaterials[ringIndex].color = new Color(color.r, color.g, color.b, 0.8f);
                ringMaterials[ringIndex].EnableKeyword("_EMISSION");
                ringMaterials[ringIndex].SetColor("_EmissionColor", color * 0.5f);
            }
        }
    }
    
    private IEnumerator PulseActiveRing(int ringIndex)
    {
        float pulseSpeed = 2f; // Adjust for faster/slower pulsing
        float minEmission = 1.0f;
        float maxEmission = 2.0f;
        
        while (ringIndex == ringOrder[currentRingIndex] && !gameCompleted) // Keep pulsing while this is the active ring
        {
            float emission = Mathf.Lerp(minEmission, maxEmission, 
                (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
                
            ringMaterials[ringIndex].SetColor("_EmissionColor", multiplayerRingColor * emission);
            
            yield return null;
        }
    }
    
    float GetRingRadius(int ringIndex)
    {
        // Use the configurable ring settings
        return minDistanceToFirstRing + baseRingRadius + (ringIndex * ringSpacing);
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