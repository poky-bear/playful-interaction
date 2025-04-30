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

        // Check parent hierarchy
        Transform parent = transform.parent;
        if (parent != null)
        {
            Debug.Log($"Object has parent: {parent.name}. This might affect local position updates.");
        }

        this.StreamingClient.RegisterRigidBody(this, RigidBodyId);
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

            // Try to update both world and local position
            transform.position = accumulatedPosition;
            transform.rotation = rbState.Pose.Orientation;

            // Store the current position for next frame's delta calculation
            lastOptitrackPosition = currentOptitrackPosition;

            // Detailed debug logging
            Debug.Log($"OptiTrack Current: {currentOptitrackPosition}, Last: {lastOptitrackPosition.Value}");
            Debug.Log($"Delta: {delta}, Accumulated: {accumulatedPosition}");
            Debug.Log($"Transform World Pos: {transform.position}, Local Pos: {transform.localPosition}");
            
            // Check if position actually changed
            if (transform.position != accumulatedPosition)
            {
                Debug.LogWarning("Position not updating as expected! Something might be overriding the transform.");
            }
        }
    }
}