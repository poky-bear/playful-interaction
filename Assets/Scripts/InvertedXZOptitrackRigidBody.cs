using UnityEngine;

/// <summary>
/// Extends OptitrackRigidBody to invert X and Z axis movement deltas.
/// </summary>
public class InvertedXZOptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public int RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;

    private Vector3? lastOptitrackPosition = null;
    private Vector3 accumulatedPosition = Vector3.zero;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.StreamingClient == null)
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.StreamingClient == null)
            {
                Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        // Check for and handle Rigidbody if present
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Make sure physics don't interfere with our position updates
            Debug.Log("Found Rigidbody, setting to kinematic to prevent physics interference");
        }

        // Check parent hierarchy and object state
        Transform parent = transform.parent;
        if (parent != null)
        {
            Debug.Log($"[HIERARCHY] Full path: {gameObject.name} -> {GetFullPath(transform)}");
            Debug.Log($"[HIERARCHY] Parent world position: {parent.position}");
            Debug.Log($"[HIERARCHY] Parent local position: {parent.localPosition}");
        }

        // Log all components on this object
        Component[] components = GetComponents<Component>();
        Debug.Log($"[COMPONENTS] Components on {gameObject.name}:");
        foreach (Component comp in components)
        {
            Debug.Log($"[COMPONENTS] - {comp.GetType().Name}");
        }

        // Check if object and its renderers are enabled
        Debug.Log($"[STATE] GameObject active: {gameObject.activeInHierarchy}, self active: {gameObject.activeSelf}");
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Debug.Log($"[STATE] Renderer '{renderer.name}' enabled: {renderer.enabled}");
        }

        this.StreamingClient.RegisterRigidBody(this, RigidBodyId);
    }

    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
    }

#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif

    void Update()
    {
        UpdatePose();
    }

    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
        if (rbState != null)
        {
            Vector3 currentOptitrackPosition = rbState.Pose.Position;

            // If this is our first position update, initialize the accumulated position
            if (!lastOptitrackPosition.HasValue)
            {
                lastOptitrackPosition = currentOptitrackPosition;
                accumulatedPosition = currentOptitrackPosition;
                transform.localPosition = accumulatedPosition;
                transform.localRotation = rbState.Pose.Orientation;
                return;
            }

            // Calculate the delta movement from OptiTrack
            Vector3 delta = currentOptitrackPosition - lastOptitrackPosition.Value;
            
            // Invert the X and Z deltas
            delta.x = -delta.x;
            delta.z = -delta.z;

            // Update the accumulated position with the inverted delta
            accumulatedPosition += delta;

            // Store previous positions for comparison
            Vector3 prevWorldPos = transform.position;
            Vector3 prevLocalPos = transform.localPosition;

            // Try to update both world and local position
            transform.position = accumulatedPosition;
            transform.rotation = rbState.Pose.Orientation;

            // Store the current position for next frame's delta calculation
            lastOptitrackPosition = currentOptitrackPosition;

            // Detailed debug logging
            Debug.Log($"[POSITION] Previous World: {prevWorldPos}, New World: {transform.position}");
            Debug.Log($"[POSITION] Previous Local: {prevLocalPos}, New Local: {transform.localPosition}");
            Debug.Log($"[OPTITRACK] Current: {currentOptitrackPosition}, Last: {lastOptitrackPosition.Value}");
            Debug.Log($"[MOVEMENT] Delta: {delta}, Accumulated: {accumulatedPosition}");
            
            // Check if position actually changed
            if (transform.position != accumulatedPosition)
            {
                Debug.LogWarning($"[WARNING] Position not updating as expected!");
                Debug.LogWarning($"[WARNING] Attempted to set position to {accumulatedPosition} but got {transform.position}");
                
                // Check if any parent transforms are moving
                Transform parent = transform.parent;
                if (parent != null)
                {
                    Debug.LogWarning($"[WARNING] Parent '{parent.name}' position: {parent.position}");
                }
            }
        }
    }
}