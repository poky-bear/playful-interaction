using UnityEngine;
using System;
using System.Collections;

public class PredatorRingGame : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Minimum distance from players to spawn target")]
    public float minSpawnDistance = 5f;
    
    [Tooltip("Maximum distance from players to spawn target")]
    public float maxSpawnDistance = 15f;
    
    [Tooltip("Color for active rings")]
    public Color activeRingColor = new Color(1f, 0.2f, 0.2f, 1f); // Red color for active ring
    
    [Tooltip("Color for inactive rings")]
    public Color inactiveRingColor = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Dark color for inactive rings
    
    [Tooltip("Tolerance for hitting the active ring")]
    public float hitTolerance = 0.5f;
    
    [Tooltip("Success message when all rings are completed")]
    public string successMessage = "Rings completed! Keep running!";
    
    [Tooltip("Distance threshold to detect player touching the target")]
    public float touchDistance = 1.0f;
    
    [Tooltip("Maximum distance player can move from rings before they disappear")]
    public float maxRingDistance = 3.0f;
    
    [Header("References")]
    [Tooltip("Reference to Player 1 sphere")]
    public GameObject player1Sphere;
    
    [Tooltip("Reference to Player 2 sphere")]
    public GameObject player2Sphere;
    
    [Tooltip("Reference to the predator object")]
    public GameObject predator;
    
    // Target dot object
    private GameObject targetDot;
    
    // Track player and position states
    private GameObject activePlayer = null;
    private Vector3 originalDotPosition;
    private Vector3? initialDotSpawnPosition = null; // Position where dot first spawned
    
    // Private variables
    private GameObject ringsObject = null;
    private GameObject[] rings;
    private Material[] ringMaterials = new Material[3];
    private Material[] originalMaterials = new Material[3];
    private int currentRingIndex = 0;
    private bool gameCompleted = false;
    private GameObject player1ExpandingCircle = null;
    private GameObject player2ExpandingCircle = null;
    private Material player1ExpandingCircleMaterial;
    private Material player2ExpandingCircleMaterial;
    private float player1CurrentRadius = 0f;
    private float player2CurrentRadius = 0f;
    private bool player1IsExpanding = false;
    private bool player2IsExpanding = false;
    private bool player1Ready = false;
    private bool player2Ready = false;
    private float expandSpeed = 1.0f; // Match RingGameController's expansion speed
    private int[] ringOrder = new int[3]; // Order in which rings should be completed
    
    void Start()
    {
        // Find references if not set
        if (player1Sphere == null) player1Sphere = GameObject.Find("Sphere");
        if (player2Sphere == null) player2Sphere = GameObject.Find("Player2Sphere");
        if (predator == null) predator = GameObject.FindGameObjectWithTag("Predator");
        
        // Create expanding circles
        CreateExpandingCircles();
        
        // Create initial target dot
        SpawnTargetDot();
    }
    
    void Update()
    {
        if (gameCompleted || player1Sphere == null || player2Sphere == null)
            return;
            
        // Check if any player is near the dot position
        Vector3 dotPosition = initialDotSpawnPosition.Value;
        float player1Distance = Vector3.Distance(player1Sphere.transform.position, dotPosition);
        float player2Distance = Vector3.Distance(player2Sphere.transform.position, dotPosition);
        
        // If no rings exist, check if either player is touching the dot
        if (ringsObject == null)
        {
            if (player1Distance <= touchDistance)
            {
                // Player 1 touched the dot first
                originalDotPosition = dotPosition;
                activePlayer = player1Sphere;
                SpawnRingsAtPosition(player1Sphere.transform.position);
                
                // Hide the dot while rings are active
                if (targetDot != null)
                {
                    Destroy(targetDot);
                    targetDot = null;
                }
            }
            else if (player2Distance <= touchDistance)
            {
                // Player 2 touched the dot first
                originalDotPosition = dotPosition;
                activePlayer = player2Sphere;
                SpawnRingsAtPosition(player2Sphere.transform.position);
                
                // Hide the dot while rings are active
                if (targetDot != null)
                {
                    Destroy(targetDot);
                    targetDot = null;
                }
            }
        }
        // If rings exist, check if active player is still in range
        else if (activePlayer != null)
        {
            float distanceFromDot = Vector3.Distance(activePlayer.transform.position, originalDotPosition);
            
            if (distanceFromDot > maxRingDistance)
            {
                // Active player moved too far from dot, destroy rings but maintain progress
                Debug.Log($"Active player moved too far from dot ({distanceFromDot:F2} units). Rings disappearing.");
                Destroy(ringsObject);
                ringsObject = null;
                rings = null;
                activePlayer = null;
                
                // Keep player ready states and currentRingIndex
                // Make the dot visible again
                CreateDotAtPosition(initialDotSpawnPosition.Value);
            }
            else
            {
                // Update rings to follow active player
                ringsObject.transform.position = activePlayer.transform.position;
                
                // Handle input for ring completion
                HandlePlayerInput();
                UpdateExpandingCircles();
            }
        }
    }
    
    void HandlePlayerInput()
    {
        // Player 1 input (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player1Ready = true;
            StartPlayer1Expanding();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            player1Ready = false;
            if (player1IsExpanding)
            {
                CheckPlayer1Hit();
            }
        }
        
        // Player 2 input (F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            player2Ready = true;
            StartPlayer2Expanding();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            player2Ready = false;
            if (player2IsExpanding)
            {
                CheckPlayer2Hit();
            }
        }
    }
    
    void UpdateExpandingCircles()
    {
        if (player1IsExpanding)
        {
            player1CurrentRadius += expandSpeed * Time.deltaTime;
            player1ExpandingCircle.transform.localScale = Vector3.one * player1CurrentRadius;
            player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        }
        
        if (player2IsExpanding)
        {
            player2CurrentRadius += expandSpeed * Time.deltaTime;
            player2ExpandingCircle.transform.localScale = Vector3.one * player2CurrentRadius;
            player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        }
        
        // Always keep circles centered on players, even when not expanding
        if (player1ExpandingCircle.activeSelf)
        {
            player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        }
        if (player2ExpandingCircle.activeSelf)
        {
            player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        }
    }
    
    void CreateDotAtPosition(Vector3 position)
    {
        if (targetDot != null)
        {
            Destroy(targetDot);
        }
        
        targetDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetDot.name = "TargetDot";
        targetDot.transform.position = position;
        targetDot.transform.localScale = Vector3.one * 0.5f;
        
        Material dotMaterial = new Material(Shader.Find("Standard"));
        dotMaterial.color = Color.red;
        dotMaterial.EnableKeyword("_EMISSION");
        dotMaterial.SetColor("_EmissionColor", Color.red * 0.5f);
        targetDot.GetComponent<Renderer>().material = dotMaterial;
        
        Destroy(targetDot.GetComponent<Collider>());
        
        Debug.Log($"[PredatorRingGame] Created dot at {position}");
    }
    
    void SpawnTargetDot()
    {
        // Get spawn position - use initial position if it exists, otherwise create new random position
        Vector3 spawnPos;
        if (initialDotSpawnPosition == null)
        {
            spawnPos = GetRandomSpawnPosition();
            initialDotSpawnPosition = spawnPos;
            Debug.Log("[PredatorRingGame] First spawn - setting initial dot position");
        }
        else
        {
            spawnPos = initialDotSpawnPosition.Value;
            Debug.Log("[PredatorRingGame] Respawning dot at initial position");
        }
        
        CreateDotAtPosition(spawnPos);
    }
    
    void SpawnRingsAtPosition(Vector3 position)
    {
        // Create rings object
        ringsObject = new GameObject("PredatorRings");
        ringsObject.transform.position = position;
        
        // Create rings
        rings = new GameObject[3];
        float baseRadius = 2.0f;
        float ringSpacing = 1.5f;
        float ringThickness = 0.2f;
        
        // Only generate new ring order if we're starting fresh
        if (currentRingIndex == 0 && !player1Ready && !player2Ready)
        {
            ringOrder = GenerateRandomOrder();
        }
        
        for (int i = 0; i < 3; i++)
        {
            float radius = baseRadius + (i * ringSpacing);
            rings[i] = new GameObject($"PredatorRing{i}");
            rings[i].transform.parent = ringsObject.transform;
            rings[i].transform.localPosition = Vector3.zero;
            
            MeshFilter meshFilter = rings[i].AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = rings[i].AddComponent<MeshRenderer>();
            
            meshFilter.mesh = CreateTorusMesh(radius, ringThickness * 0.5f);
            
            MeshCollider meshCollider = rings[i].AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshCollider.convex = true;
            meshCollider.isTrigger = true;
            
            Material ringMaterial = new Material(Shader.Find("Standard"));
            SetupTransparentMaterial(ringMaterial);
            
            // Set color based on current progress
            Color initialColor;
            bool isCompleted = false;
            
            // Check if this ring was already completed
            for (int j = 0; j < currentRingIndex; j++)
            {
                if (ringOrder[j] == i)
                {
                    isCompleted = true;
                    break;
                }
            }
            
            if (i == ringOrder[currentRingIndex])
            {
                // This is the current active ring
                initialColor = activeRingColor;
            }
            else if (isCompleted)
            {
                // This ring was already completed
                initialColor = inactiveRingColor;
            }
            else
            {
                // This ring hasn't been activated yet
                initialColor = inactiveRingColor;
            }
            ringMaterial.color = initialColor;
            ringMaterial.EnableKeyword("_EMISSION");
            ringMaterial.SetColor("_EmissionColor", initialColor * 0.3f);
            
            originalMaterials[i] = ringMaterial;
            ringMaterials[i] = new Material(ringMaterial);
            rings[i].GetComponent<Renderer>().material = ringMaterials[i];
        }
        
        Debug.Log($"[PredatorRingGame] Spawned new rings at player position. Ring order: {ringOrder[0]}, {ringOrder[1]}, {ringOrder[2]}");
    }
    
    int[] GenerateRandomOrder()
    {
        int[] order = { 0, 1, 2 };
        
        // Fisher-Yates shuffle
        for (int i = order.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
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
            
            // Set emission for better visibility
            ringMaterials[ringIndex].EnableKeyword("_EMISSION");
            ringMaterials[ringIndex].SetColor("_EmissionColor", color * 0.3f);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Define the play area bounds (assuming walls form a square)
        const float ARENA_SIZE = 20f; // Adjust this based on actual wall positions
        const float WALL_MARGIN = 2f; // Keep some distance from walls
        
        // Get the midpoint between players as a reference point
        Vector3 playersCenter = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        
        // Calculate bounds relative to center
        float minX = Mathf.Max(-ARENA_SIZE/2 + WALL_MARGIN, playersCenter.x - maxSpawnDistance);
        float maxX = Mathf.Min(ARENA_SIZE/2 - WALL_MARGIN, playersCenter.x + maxSpawnDistance);
        float minZ = Mathf.Max(-ARENA_SIZE/2 + WALL_MARGIN, playersCenter.z - maxSpawnDistance);
        float maxZ = Mathf.Min(ARENA_SIZE/2 - WALL_MARGIN, playersCenter.z + maxSpawnDistance);
        
        // Get random position within bounds
        float x = UnityEngine.Random.Range(minX, maxX);
        float z = UnityEngine.Random.Range(minZ, maxZ);
        
        // Keep y position at player level
        float y = player1Sphere.transform.position.y;
        
        // Create position
        Vector3 position = new Vector3(x, y, z);
        
        // Ensure minimum distance from players
        Vector3 toPlayer1 = position - player1Sphere.transform.position;
        Vector3 toPlayer2 = position - player2Sphere.transform.position;
        
        // If too close to either player, move the position away
        if (toPlayer1.magnitude < minSpawnDistance || toPlayer2.magnitude < minSpawnDistance)
        {
            // Get direction from closest player
            Vector3 awayDir = toPlayer1.magnitude < toPlayer2.magnitude ? toPlayer1 : toPlayer2;
            awayDir.y = 0; // Keep movement in XZ plane
            awayDir = awayDir.normalized;
            
            // Move position away until minimum distance is met
            position = position + awayDir * minSpawnDistance;
            
            // Clamp to arena bounds
            position.x = Mathf.Clamp(position.x, minX, maxX);
            position.z = Mathf.Clamp(position.z, minZ, maxZ);
        }
        
        Debug.Log($"[PredatorRingGame] Spawning dot at {position}, Arena bounds: {ARENA_SIZE}x{ARENA_SIZE}");
        return position;
    }
    
    void CreateExpandingCircles()
    {
        // Player 1's expanding circle
        player1ExpandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player1ExpandingCircle.name = "Player1ExpandingCircle";
        player1ExpandingCircle.transform.localScale = Vector3.zero;
        player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        Destroy(player1ExpandingCircle.GetComponent<Collider>());
        
        player1ExpandingCircleMaterial = new Material(Shader.Find("Standard"));
        SetupTransparentMaterial(player1ExpandingCircleMaterial);
        player1ExpandingCircleMaterial.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        player1ExpandingCircle.GetComponent<Renderer>().material = player1ExpandingCircleMaterial;
        player1ExpandingCircle.SetActive(false);
        
        // Player 2's expanding circle
        player2ExpandingCircle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player2ExpandingCircle.name = "Player2ExpandingCircle";
        player2ExpandingCircle.transform.localScale = Vector3.zero;
        player2ExpandingCircle.transform.position = player2Sphere.transform.position;
        Destroy(player2ExpandingCircle.GetComponent<Collider>());
        
        player2ExpandingCircleMaterial = new Material(Shader.Find("Standard"));
        SetupTransparentMaterial(player2ExpandingCircleMaterial);
        player2ExpandingCircleMaterial.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        player2ExpandingCircle.GetComponent<Renderer>().material = player2ExpandingCircleMaterial;
        player2ExpandingCircle.SetActive(false);
    }
    
    void SetupTransparentMaterial(Material material)
    {
        material.SetFloat("_Mode", 3);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
    }
    
    private System.Collections.IEnumerator HideExpandingCircleAfterDelay(GameObject circle, Material material, float delay)
    {
        // Keep the circle visible for the specified delay
        yield return new WaitForSeconds(delay);
        
        // Fade out the circle
        float fadeOutDuration = 0.25f;
        float time = 0;
        Color currentColor = material.color;
        
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(currentColor.a, 0f, time / fadeOutDuration);
            
            material.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
            
            yield return null;
        }
        
        // Disable emission and hide the circle
        material.DisableKeyword("_EMISSION");
        circle.SetActive(false);
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
                vertices[vertIndex] = new Vector3(x, z, y);
                uv[vertIndex] = new Vector2((float)i / tubularSegments, (float)j / radialSegments);
            }
        }
        
        int index = 0;
        for (int i = 0; i < tubularSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int a = i * (radialSegments + 1) + j;
                int b = i * (radialSegments + 1) + j + 1;
                int c = (i + 1) * (radialSegments + 1) + j + 1;
                int d = (i + 1) * (radialSegments + 1) + j;
                
                triangles[index++] = a;
                triangles[index++] = b;
                triangles[index++] = d;
                
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
    
    void StartPlayer1Expanding()
    {
        player1CurrentRadius = 0.1f; // Start with small radius like RingGameController
        player1IsExpanding = true;
        
        // Set a visible gray color for the expanding circle
        player1ExpandingCircleMaterial.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        player1ExpandingCircleMaterial.DisableKeyword("_EMISSION");
        
        player1ExpandingCircle.SetActive(true);
        player1ExpandingCircle.transform.position = player1Sphere.transform.position;
    }
    
    void StartPlayer2Expanding()
    {
        player2CurrentRadius = 0.1f; // Start with small radius like RingGameController
        player2IsExpanding = true;
        
        // Set a visible gray color for the expanding circle
        player2ExpandingCircleMaterial.color = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        player2ExpandingCircleMaterial.DisableKeyword("_EMISSION");
        
        player2ExpandingCircle.SetActive(true);
        player2ExpandingCircle.transform.position = player2Sphere.transform.position;
    }
    
    void CheckPlayer1Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        // Get the radius of the current active ring
        float ringRadius = 2.0f + (ringOrder[currentRingIndex] * 1.5f);
        
        // The actual radius is half the scale since we're using a sphere
        float actualRadius = player1CurrentRadius / 2f;
        
        // Calculate distance from target for feedback
        float distanceFromTarget = Mathf.Abs(actualRadius - ringRadius);
        
        // Check if the expanding circle matches the ring size
        if (distanceFromTarget <= hitTolerance)
        {
            // Successful hit - change color to green
            player1ExpandingCircleMaterial.color = new Color(0f, 1f, 0.2f, 0.5f);
            player1ExpandingCircleMaterial.EnableKeyword("_EMISSION");
            player1ExpandingCircleMaterial.SetColor("_EmissionColor", new Color(0f, 1f, 0.2f) * 0.5f);
            
            player1Ready = true;
            Debug.Log($"[Player 1] Hit! Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            
            // If both players have hit, advance to next ring
            if (player2Ready)
            {
                AdvanceToNextRing();
            }
        }
        else
        {
            // Show feedback on how close they were
            if (distanceFromTarget < hitTolerance * 2)
            {
                // Close - orange color
                player1ExpandingCircleMaterial.color = new Color(1f, 0.6f, 0f, 0.5f);
                player1ExpandingCircleMaterial.EnableKeyword("_EMISSION");
                player1ExpandingCircleMaterial.SetColor("_EmissionColor", new Color(1f, 0.6f, 0f) * 0.3f);
                
                Debug.Log($"[Player 1] Close! {distanceFromTarget:F2} units away. Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            }
            else
            {
                // Miss - red color
                player1ExpandingCircleMaterial.color = new Color(1f, 0.2f, 0.2f, 0.5f);
                player1ExpandingCircleMaterial.DisableKeyword("_EMISSION");
                
                Debug.Log($"[Player 1] Miss! {distanceFromTarget:F2} units away. Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            }
        }
        
        // Keep the expanding circle visible for feedback
        StartCoroutine(HideExpandingCircleAfterDelay(player1ExpandingCircle, player1ExpandingCircleMaterial, 1.0f));
        
        // Reset expansion state
        player1CurrentRadius = 0f;
        player1IsExpanding = false;
    }
    
    void CheckPlayer2Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        // Get the radius of the current active ring
        float ringRadius = 2.0f + (ringOrder[currentRingIndex] * 1.5f);
        
        // The actual radius is half the scale since we're using a sphere
        float actualRadius = player2CurrentRadius / 2f;
        
        // Calculate distance from target for feedback
        float distanceFromTarget = Mathf.Abs(actualRadius - ringRadius);
        
        // Check if the expanding circle matches the ring size
        if (distanceFromTarget <= hitTolerance)
        {
            // Successful hit - change color to green
            player2ExpandingCircleMaterial.color = new Color(0f, 1f, 0.2f, 0.5f);
            player2ExpandingCircleMaterial.EnableKeyword("_EMISSION");
            player2ExpandingCircleMaterial.SetColor("_EmissionColor", new Color(0f, 1f, 0.2f) * 0.5f);
            
            player2Ready = true;
            Debug.Log($"[Player 2] Hit! Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            
            // If both players have hit, advance to next ring
            if (player1Ready)
            {
                AdvanceToNextRing();
            }
        }
        else
        {
            // Show feedback on how close they were
            if (distanceFromTarget < hitTolerance * 2)
            {
                // Close - orange color
                player2ExpandingCircleMaterial.color = new Color(1f, 0.6f, 0f, 0.5f);
                player2ExpandingCircleMaterial.EnableKeyword("_EMISSION");
                player2ExpandingCircleMaterial.SetColor("_EmissionColor", new Color(1f, 0.6f, 0f) * 0.3f);
                
                Debug.Log($"[Player 2] Close! {distanceFromTarget:F2} units away. Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            }
            else
            {
                // Miss - red color
                player2ExpandingCircleMaterial.color = new Color(1f, 0.2f, 0.2f, 0.5f);
                player2ExpandingCircleMaterial.DisableKeyword("_EMISSION");
                
                Debug.Log($"[Player 2] Miss! {distanceFromTarget:F2} units away. Radius: {actualRadius:F2}, Target: {ringRadius:F2}");
            }
        }
        
        // Keep the expanding circle visible for feedback
        StartCoroutine(HideExpandingCircleAfterDelay(player2ExpandingCircle, player2ExpandingCircleMaterial, 1.0f));
        
        // Reset expansion state
        player2CurrentRadius = 0f;
        player2IsExpanding = false;
    }
    
    void AdvanceToNextRing()
    {
        // Both players hit the current ring
        int currentRing = ringOrder[currentRingIndex];
        SetRingColor(currentRing, inactiveRingColor); // Set completed ring to inactive
        
        currentRingIndex++;
        player1Ready = false;
        player2Ready = false;
        
        if (currentRingIndex >= rings.Length)
        {
            // All rings completed
            Debug.Log("All rings completed!");
            OnRingsCompleted();
        }
        else
        {
            // Activate next ring in sequence
            int nextRing = ringOrder[currentRingIndex];
            SetRingColor(nextRing, activeRingColor);
            Debug.Log($"Ring {currentRing} completed! Next ring: {nextRing}");
        }
    }
    
    void CheckRingCompletion()
    {
        if (currentRingIndex < rings.Length)
        {
            // Check if both players have hit the current ring
            if (player1Ready && player2Ready)
            {
                // Both players have hit the current ring
                int currentRing = ringOrder[currentRingIndex];
                SetRingColor(currentRing, inactiveRingColor); // Set completed ring to inactive
                
                currentRingIndex++;
                player1Ready = false;
                player2Ready = false;
                
                if (currentRingIndex >= rings.Length)
                {
                    // All rings completed
                    Debug.Log("All rings completed!");
                    OnRingsCompleted();
                }
                else
                {
                    // Activate next ring in sequence
                    int nextRing = ringOrder[currentRingIndex];
                    SetRingColor(nextRing, activeRingColor);
                    Debug.Log($"Ring {currentRing} completed! Next ring: {nextRing}");
                }
            }
            else
            {
                // Log which player has hit the current ring
                if (player1Ready && !player2Ready)
                {
                    Debug.Log("Player 1 hit the ring. Waiting for Player 2.");
                }
                else if (!player1Ready && player2Ready)
                {
                    Debug.Log("Player 2 hit the ring. Waiting for Player 1.");
                }
            }
        }
    }
    
    void OnRingsCompleted()
    {
        gameCompleted = true;
        Debug.Log("All rings completed successfully! Predator defeated!");
        
        // Destroy current rings
        if (ringsObject != null)
        {
            Destroy(ringsObject);
            ringsObject = null;
            rings = null;
        }
        
        // Hide the predator
        if (predator != null)
        {
            predator.SetActive(false);
        }
        
        // Reset game state
        // gameCompleted = false;
        currentRingIndex = 0;
        player1Ready = false;
        player2Ready = false;
        activePlayer = null;
        
        // Make the original dot visible again
        CreateDotAtPosition(initialDotSpawnPosition.Value);
    }
    
    public void OnPredatorModeActivated()
    {
        gameCompleted = false;
        currentRingIndex = 0;
        player1Ready = false;
        player2Ready = false;
        SpawnTargetDot();
    }
    
    public void OnPredatorModeDeactivated()
    {
        if (ringsObject != null)
        {
            Destroy(ringsObject);
            ringsObject = null;
            rings = null;
        }
        if (targetDot != null)
        {
            Destroy(targetDot);
            targetDot = null;
        }
        activePlayer = null;
        initialDotSpawnPosition = null; // Reset initial spawn position for next activation
    }
}