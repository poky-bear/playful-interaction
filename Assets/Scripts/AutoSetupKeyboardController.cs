using UnityEngine;

public class AutoSetupKeyboardController : MonoBehaviour
{
    void Awake()
    {
        // Find the KeyboardSphere
        GameObject keyboardSphere = GameObject.Find("KeyboardSphere");
        if (keyboardSphere == null)
        {
            Debug.LogError("KeyboardSphere not found in the scene!");
            return;
        }
        
        // Add KeyboardController component if it doesn't exist
        KeyboardController controller = keyboardSphere.GetComponent<KeyboardController>();
        if (controller == null)
        {
            controller = keyboardSphere.AddComponent<KeyboardController>();
            Debug.Log("Added KeyboardController component to KeyboardSphere");
            
            // Set default values
            controller.moveSpeed = 10f;
            controller.maxSpeed = 15f;
            controller.usePhysics = true;
            controller.heightConstraint = 1.58f; // Based on the y position in the scene
            controller.drag = 0.5f;
        }
        
        // Add Rigidbody if needed
        Rigidbody rb = keyboardSphere.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = keyboardSphere.AddComponent<Rigidbody>();
            rb.drag = controller.drag;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            Debug.Log("Added Rigidbody component to KeyboardSphere");
        }
        
        Debug.Log("Keyboard controller setup complete!");
        
        // Remove this component after setup
        Destroy(this);
    }
}