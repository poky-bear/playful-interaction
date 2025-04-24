using UnityEngine;

public class PredatorRingGame : MonoBehaviour
{
    [Header("Ring Settings")]
    [Tooltip("Minimum distance from players to spawn rings")]
    public float minSpawnDistance = 5f;
    
    [Tooltip("Maximum distance from players to spawn rings")]
    public float maxSpawnDistance = 15f;
    
    [Tooltip("Color for the predator mode rings")]
    public Color ringColor = new Color(1f, 0.2f, 0.2f, 1f); // Red color for predator mode
    
    [Tooltip("Success message when all rings are completed")]
    public string successMessage = "Rings completed! Keep running!";
    
    [Header("References")]
    [Tooltip("Reference to Player 1 sphere")]
    public GameObject player1Sphere;
    
    [Tooltip("Reference to Player 2 sphere")]
    public GameObject player2Sphere;
    
    [Tooltip("Reference to the predator object")]
    public GameObject predator;
    
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
    private float expandSpeed = 5f;
    private float maxRadius = 5f;
    
    void Start()
    {
        // Find references if not set
        if (player1Sphere == null) player1Sphere = GameObject.Find("Sphere");
        if (player2Sphere == null) player2Sphere = GameObject.Find("Player2Sphere");
        if (predator == null) predator = GameObject.FindGameObjectWithTag("Predator");
        
        // Create expanding circles
        CreateExpandingCircles();
        
        // Create initial set of rings
        SpawnNewRings();
    }
    
    void Update()
    {
        if (gameCompleted || player1Sphere == null || player2Sphere == null)
            return;
            
        // Update ring positions
        if (ringsObject != null)
        {
            // Keep rings stationary, they were spawned at a random position
        }
        
        // Handle player input and expanding circles
        HandlePlayerInput();
        UpdateExpandingCircles();
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
            if (player1CurrentRadius > maxRadius)
            {
                player1CurrentRadius = 0f;
                player1IsExpanding = false;
                player1ExpandingCircle.SetActive(false);
            }
            player1ExpandingCircle.transform.localScale = Vector3.one * player1CurrentRadius * 2f;
        }
        
        if (player2IsExpanding)
        {
            player2CurrentRadius += expandSpeed * Time.deltaTime;
            if (player2CurrentRadius > maxRadius)
            {
                player2CurrentRadius = 0f;
                player2IsExpanding = false;
                player2ExpandingCircle.SetActive(false);
            }
            player2ExpandingCircle.transform.localScale = Vector3.one * player2CurrentRadius * 2f;
        }
        
        // Update expanding circle positions
        if (player1ExpandingCircle != null)
            player1ExpandingCircle.transform.position = player1Sphere.transform.position;
        if (player2ExpandingCircle != null)
            player2ExpandingCircle.transform.position = player2Sphere.transform.position;
    }
    
    void SpawnNewRings()
    {
        // Calculate spawn position
        Vector3 spawnPos = GetRandomSpawnPosition();
        
        // Create rings object
        ringsObject = new GameObject("PredatorRings");
        ringsObject.transform.position = spawnPos;
        
        // Create rings
        rings = new GameObject[3];
        float baseRadius = 2.0f;
        float ringSpacing = 1.5f;
        float ringThickness = 0.2f;
        
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
            
            Color inactiveColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            ringMaterial.color = inactiveColor;
            ringMaterial.EnableKeyword("_EMISSION");
            ringMaterial.SetColor("_EmissionColor", inactiveColor * 0.3f);
            
            originalMaterials[i] = ringMaterial;
            ringMaterials[i] = new Material(ringMaterial);
            rings[i].GetComponent<Renderer>().material = ringMaterials[i];
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
        player1CurrentRadius = 0f;
        player1IsExpanding = true;
        player1ExpandingCircle.SetActive(true);
    }
    
    void StartPlayer2Expanding()
    {
        player2CurrentRadius = 0f;
        player2IsExpanding = true;
        player2ExpandingCircle.SetActive(true);
    }
    
    void CheckPlayer1Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        float ringRadius = 2.0f + (currentRingIndex * 1.5f);
        float tolerance = 0.3f;
        
        if (Mathf.Abs(player1CurrentRadius - ringRadius) <= tolerance)
        {
            player1Ready = true;
            CheckRingCompletion();
        }
        
        player1CurrentRadius = 0f;
        player1IsExpanding = false;
        player1ExpandingCircle.SetActive(false);
    }
    
    void CheckPlayer2Hit()
    {
        if (currentRingIndex >= rings.Length) return;
        
        float ringRadius = 2.0f + (currentRingIndex * 1.5f);
        float tolerance = 0.3f;
        
        if (Mathf.Abs(player2CurrentRadius - ringRadius) <= tolerance)
        {
            player2Ready = true;
            CheckRingCompletion();
        }
        
        player2CurrentRadius = 0f;
        player2IsExpanding = false;
        player2ExpandingCircle.SetActive(false);
    }
    
    void CheckRingCompletion()
    {
        if (player1Ready && player2Ready && currentRingIndex < rings.Length)
        {
            // Both players hit the current ring correctly
            ringMaterials[currentRingIndex].color = ringColor;
            ringMaterials[currentRingIndex].SetColor("_EmissionColor", ringColor * 0.5f);
            
            currentRingIndex++;
            player1Ready = false;
            player2Ready = false;
            
            if (currentRingIndex >= rings.Length)
            {
                // All rings completed
                OnRingsCompleted();
            }
        }
    }
    
    void OnRingsCompleted()
    {
        gameCompleted = true;
        Debug.Log(successMessage);
        
        // Spawn new set of rings
        if (ringsObject != null)
        {
            Destroy(ringsObject);
        }
        
        gameCompleted = false;
        currentRingIndex = 0;
        SpawnNewRings();
    }
    
    public void OnPredatorModeActivated()
    {
        gameCompleted = false;
        currentRingIndex = 0;
        SpawnNewRings();
    }
    
    public void OnPredatorModeDeactivated()
    {
        if (ringsObject != null)
        {
            Destroy(ringsObject);
        }
    }
}