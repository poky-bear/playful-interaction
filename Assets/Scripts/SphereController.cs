using UnityEngine;

public class SphereController : MonoBehaviour
{
    public float moveSpeed = 10f;        // How fast the sphere moves
    public float smoothness = 0.1f;      // Lower = more responsive, Higher = more smooth
    public float heightAboveGround = 1f; // Height the sphere maintains above the ground
    public LayerMask groundLayer;        // Layer mask for the ground/floor

    private Camera mainCamera;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;

    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;
    }

    void Update()
    {
        // Cast a ray from the camera through the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // If the ray hits something on the ground layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            // Set target position at hit point, but maintain height above ground
            targetPosition = hit.point + Vector3.up * heightAboveGround;
        }

        // Smoothly move the sphere towards the target position
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref currentVelocity, 
            smoothness, 
            moveSpeed
        );
    }
}
}