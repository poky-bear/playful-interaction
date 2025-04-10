using UnityEngine;

public class Player2Controller : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float maxSpeed = 15f;
    public bool usePhysics = true;
    public float heightConstraint = 0f; // If > 0, constrains the y position

    [Header("Physics Settings")]
    public float drag = 0.5f;
    
    [Header("Custom Input Keys")]
    public KeyCode moveLeftKey = KeyCode.J;
    public KeyCode moveRightKey = KeyCode.L;
    public KeyCode moveForwardKey = KeyCode.I;
    public KeyCode moveBackwardKey = KeyCode.K;
    
    private Rigidbody rb;
    private Vector3 movement;
    
    void Start()
    {
        // Get or add a Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null && usePhysics)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.drag = drag;
            rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent the sphere from rotating
        }
    }
    
    void Update()
    {
        // Get input from custom keys (default to IJKL instead of WASD)
        float horizontalInput = 0f;
        float verticalInput = 0f;
        
        // Manual input detection for custom keys
        if (Input.GetKey(moveLeftKey)) horizontalInput -= 1f;
        if (Input.GetKey(moveRightKey)) horizontalInput += 1f;
        if (Input.GetKey(moveBackwardKey)) verticalInput -= 1f;
        if (Input.GetKey(moveForwardKey)) verticalInput += 1f;
        
        // Normalize the movement vector to prevent diagonal movement being faster
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        }
        else
        {
            movement = Vector3.zero;
        }
        
        // If not using physics, move directly
        if (!usePhysics)
        {
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
            
            // Apply height constraint if specified
            if (heightConstraint > 0)
            {
                Vector3 pos = transform.position;
                pos.y = heightConstraint;
                transform.position = pos;
            }
        }
    }
    
    void FixedUpdate()
    {
        // Apply physics-based movement if using Rigidbody
        if (usePhysics && rb != null)
        {
            // Apply force in the direction of movement
            rb.AddForce(movement * moveSpeed, ForceMode.Acceleration);
            
            // Limit maximum speed
            if (rb.velocity.magnitude > maxSpeed)
            {
                rb.velocity = rb.velocity.normalized * maxSpeed;
            }
            
            // Apply height constraint if specified
            if (heightConstraint > 0)
            {
                Vector3 pos = rb.position;
                pos.y = heightConstraint;
                rb.position = pos;
                
                // Zero out vertical velocity
                Vector3 vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
            }
        }
    }
}