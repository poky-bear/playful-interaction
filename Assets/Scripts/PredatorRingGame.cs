using UnityEngine;

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
    
    // Track which player activated the rings and the original dot position
    private GameObject activePlayer = null;
    private Vector3 originalDotPosition;
    
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
            
        // Check if any player has touched the target dot
        if (targetDot != null)
        {
            Vector3 dotPosition = targetDot.transform.position;
            float player1Distance = Vector3.Distance(player1Sphere.transform.position, dotPosition);
            float player2Distance = Vector3.Distance(player2Sphere.transform.position, dotPosition);
            
            if (player1Distance <= touchDistance)
            {
                // Player 1 touched the dot
                originalDotPosition = dotPosition;
                Destroy(targetDot);
                targetDot = null;
                activePlayer = player1Sphere;
                SpawnRingsAtPosition(player1Sphere.transform.position);
            }
            else if (player2Distance <= touchDistance)
            {
                // Player 2 touched the dot
                originalDotPosition = dotPosition;
                Destroy(targetDot);
                targetDot = null;
                activePlayer = player2Sphere;
                SpawnRingsAtPosition(player2Sphere.transform.position);
            }
        }
        
        // Check if active player has moved too far from original dot position
        if (ringsObject != null && activePlayer != null)
        {
            float distanceFromDot = Vector3.Distance(activePlayer.transform.position, originalDotPosition);
            
            if (distanceFromDot > maxRingDistance)
            {
                // Player moved too far from dot position, destroy rings and respawn dot
                Debug.Log($"Player moved too far from dot ({distanceFromDot:F2} units). Rings disappearing.");
                Destroy(ringsObject);
                ringsObject = null;
                rings = null;
                activePlayer = null;
                SpawnTargetDot();
            }
            else
            {
                // Update rings position to follow player
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
    
    void SpawnTargetDot()
    {
        // Calculate spawn position
        Vector3 spawnPos = GetRandomSpawnPosition();
        
        // Create target dot
        targetDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetDot.name = "TargetDot";
        targetDot.transform.position = spawnPos;
        targetDot.transform.localScale = Vector3.one * 0.5f; // Make it smaller than players
        
        // Create and set material
        Material dotMaterial = new Material(Shader.Find("Standard"));
        dotMaterial.color = Color.red;
        dotMaterial.EnableKeyword("_EMISSION");
        dotMaterial.SetColor("_EmissionColor", Color.red * 0.5f);
        targetDot.GetComponent<Renderer>().material = dotMaterial;
        
        // Remove collider as we'll use distance check instead
        Destroy(targetDot.GetComponent<Collider>());
        
        Debug.Log("[PredatorRingGame] Spawned target dot");
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
        
        // Generate random order for rings
        ringOrder = GenerateRandomOrder();
        currentRingIndex = 0;
        
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
            
            // Set initial color based on whether this is the first active ring
            Color initialColor = (i == ringOrder[0]) ? activeRingColor : inactiveRingColor;
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
            
            // Set emission for better visibility
            ringMaterials[ringIndex].EnableKeyword("_EMISSION");
            ringMaterials[ringIndex].SetColor("_EmissionColor", color * 0.3f);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Get the midpoint between players
        Vector3 playersCenter = (player1Sphere.transform.position + player2Sphere.transform.position) / 2f;
        
        // Get a random angle
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Get a random distance between min and max
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // Calculate position
        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );
        
        return playersCenter + offset;
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
        player1ExpandingCircleMaterial.color = new Color(1f, 0.8f, 0.2f, 0.5f);
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
        player2ExpandingCircleMaterial.color = new Color(0.2f, 0.8f, 1f, 0.5f);
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
        player1ExpandingCircle.SetActive(true);
        player1ExpandingCircle.transform.position = player1Sphere.transform.position;
    }
    
    void StartPlayer2Expanding()
    {
        player2CurrentRadius = 0.1f; // Start with small radius like RingGameController
        player2IsExpanding = true;
        player2ExpandingCircle.SetActive(true);
        player2ExpandingCircle.transform.position = player2Sphere.transform.position;
    }
    
    void CheckPlayer1Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        // Get the radius of the current active ring
        float ringRadius = 2.0f + (ringOrder[currentRingIndex] * 1.5f);
        
        // Check if the expanding circle matches the ring size
        if (Mathf.Abs(player1CurrentRadius - ringRadius) <= hitTolerance)
        {
            player1Ready = true;
            CheckRingCompletion();
        }
        else
        {
            // Show feedback on how close they were
            float distanceFromTarget = Mathf.Abs(player1CurrentRadius - ringRadius);
            if (distanceFromTarget < hitTolerance * 2)
            {
                Debug.Log($"[Player 1] Close! {distanceFromTarget:F2} units away. Tolerance: {hitTolerance}");
            }
            else
            {
                Debug.Log($"[Player 1] Miss! {distanceFromTarget:F2} units away. Tolerance: {hitTolerance}");
            }
        }
        
        // Reset player 1's expanding circle
        player1CurrentRadius = 0f;
        player1IsExpanding = false;
        player1ExpandingCircle.SetActive(false);
    }
    
    void CheckPlayer2Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        // Get the radius of the current active ring
        float ringRadius = 2.0f + (ringOrder[currentRingIndex] * 1.5f);
        
        // Check if the expanding circle matches the ring size
        if (Mathf.Abs(player2CurrentRadius - ringRadius) <= hitTolerance)
        {
            player2Ready = true;
            CheckRingCompletion();
        }
        else
        {
            // Show feedback on how close they were
            float distanceFromTarget = Mathf.Abs(player2CurrentRadius - ringRadius);
            if (distanceFromTarget < hitTolerance * 2)
            {
                Debug.Log($"[Player 2] Close! {distanceFromTarget:F2} units away. Tolerance: {hitTolerance}");
            }
            else
            {
                Debug.Log($"[Player 2] Miss! {distanceFromTarget:F2} units away. Tolerance: {hitTolerance}");
            }
        }
        
        // Reset player 2's expanding circle
        player2CurrentRadius = 0f;
        player2IsExpanding = false;
        player2ExpandingCircle.SetActive(false);
    }
    
    void CheckRingCompletion()
    {
        if (player1Ready && player2Ready && currentRingIndex < rings.Length)
        {
            // Both players hit the current ring correctly
            int currentRing = ringOrder[currentRingIndex];
            SetRingColor(currentRing, activeRingColor); // Set current ring to active color
            
            currentRingIndex++;
            player1Ready = false;
            player2Ready = false;
            
            if (currentRingIndex >= rings.Length)
            {
                // All rings completed
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
    }
    
    void OnRingsCompleted()
    {
        gameCompleted = true;
        Debug.Log(successMessage);
        
        // Destroy current rings
        if (ringsObject != null)
        {
            Destroy(ringsObject);
            ringsObject = null;
            rings = null;
        }
        
        // Reset game state
        gameCompleted = false;
        currentRingIndex = 0;
        player1Ready = false;
        player2Ready = false;
        activePlayer = null;
        
        // Spawn new target dot
        SpawnTargetDot();
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
    }
}