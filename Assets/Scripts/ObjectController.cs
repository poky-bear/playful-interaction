using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float smoothness = 0.1f;
    public float heightOffset = 10f;  // Height at which the object will follow the mouse
    public LayerMask groundLayer;     // Layer for the ground/plane to raycast against
    
    private Vector3 velocity = Vector3.zero;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Get mouse position and convert to world space
        Vector3 mousePos = Input.mousePosition;
        
        // Create a ray from the camera through the mouse position
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        Vector3 targetPosition;

        // If we hit something with our raycast, use that point
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = hit.point + Vector3.up * heightOffset;
        }
        else
        {
            // If we don't hit anything, project the mouse position to a plane at heightOffset
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.up * heightOffset);
            float distance;
            if (horizontalPlane.Raycast(ray, out distance))
            {
                targetPosition = ray.GetPoint(distance);
            }
            else
            {
                return; // Skip this frame if we can't determine a target position
            }
        }

        // Apply smooth movement
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothness
        );
    }
}