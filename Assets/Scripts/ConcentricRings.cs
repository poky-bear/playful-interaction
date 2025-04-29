using UnityEngine;

public class ConcentricRings : MonoBehaviour
{
    [Header("Ring Settings")]
    [Tooltip("Minimum distance between sphere and first ring")]
    public float minDistanceToFirstRing = 1.0f;
    
    [Tooltip("Distance between each ring")]
    public float ringSpacing = 1.0f;
    
    [Tooltip("Thickness of each ring")]
    public float ringThickness = 0.1f;
    
    [Header("Ring Appearance")]
    public Material[] ringMaterials;
    
    [Header("References")]
    [Tooltip("Reference to the sphere - if null, will use this GameObject")]
    public GameObject targetSphere;
    
    // Made public to allow access from other scripts
    [HideInInspector]
    public GameObject[] rings = new GameObject[3];
    
    // Made public to allow access from other scripts
    [HideInInspector]
    public float sphereRadius = 0.5f; // Default Unity sphere radius
    
    void Start()
    {
        // If no target sphere is assigned, use this GameObject
        if (targetSphere == null)
        {
            targetSphere = gameObject;
        }
        
        // Get the sphere's radius from its collider if available
        SphereCollider sphereCollider = targetSphere.GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            sphereRadius = sphereCollider.radius * Mathf.Max(
                targetSphere.transform.localScale.x,
                targetSphere.transform.localScale.y,
                targetSphere.transform.localScale.z
            );
        }
        
        CreateRings();
    }
    
    void CreateRings()
    {
        // Create a parent object to hold all rings
        GameObject ringsParent = new GameObject("ConcentricRings");
        ringsParent.transform.position = targetSphere.transform.position;
        ringsParent.transform.parent = transform;
        
        // Create three rings with increasing radii
        for (int i = 0; i < 3; i++)
        {
            float ringRadius = sphereRadius + minDistanceToFirstRing + (i * ringSpacing);
            rings[i] = CreateRing(ringRadius, i);
            rings[i].transform.parent = ringsParent.transform;
            rings[i].transform.localPosition = Vector3.zero;
            
            // Apply material if available
            if (ringMaterials != null && ringMaterials.Length > i && ringMaterials[i] != null)
            {
                rings[i].GetComponent<Renderer>().material = ringMaterials[i];
            }
        }
    }
    
    GameObject CreateRing(float radius, int index)
    {
        // Create a ring using a torus primitive
        GameObject ring = new GameObject("Ring_" + (index + 1));
        
        // Add a mesh filter and renderer
        MeshFilter meshFilter = ring.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = ring.AddComponent<MeshRenderer>();
        
        // Generate torus mesh
        meshFilter.mesh = CreateTorusMesh(radius, ringThickness);
        
        // Add a collider (optional)
        MeshCollider meshCollider = ring.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.mesh;
        meshCollider.convex = true; // Required for triggers
        meshCollider.isTrigger = true; // Make it a trigger collider so it doesn't block movement
        
        return ring;
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
    
    // Update the rings position if the sphere moves
    void LateUpdate()
    {
        if (rings[0] != null && rings[0].transform.parent != null)
        {
            rings[0].transform.parent.position = targetSphere.transform.position;
        }
    }
}