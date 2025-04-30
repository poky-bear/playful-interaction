using System.IO.Compression;
using UnityEngine;

/// <summary>
/// Extends OptitrackRigidBody to invert X and Z-axis movement deltas.
/// </summary>
public class InvertedZOptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public int RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;

    private Vector3? lastOptitrackPosition = null;
    private Vector3 accumulatedPosition;

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
            Debug.Log($"[Raw OptiTrack] Frame: {Time.frameCount}, Position: {currentOptitrackPosition}, LastPosition: {lastOptitrackPosition}");

            // If this is our first position update, initialize the accumulated position
            if (!lastOptitrackPosition.HasValue)
            {
                lastOptitrackPosition = currentOptitrackPosition;
                // Keep the initial position as-is from OptiTrack
                accumulatedPosition = currentOptitrackPosition;
                transform.localPosition = accumulatedPosition;
                transform.localRotation = rbState.Pose.Orientation;
                return;
            }

            // Calculate the delta movement from OptiTrack
            Vector3 delta = currentOptitrackPosition - lastOptitrackPosition.Value;
            
            // Scale up small movements to make them more noticeable
            // Adjust this multiplier as needed
            const float movementMultiplier = 5.0f;
            delta *= movementMultiplier;
            
            // Invert the X and Z deltas
            delta.x = -delta.x;
            delta.z = -delta.z;

            // Update the accumulated position with the inverted deltas
            accumulatedPosition += delta;

            // Update the transform
            transform.localPosition = accumulatedPosition;
            transform.localRotation = rbState.Pose.Orientation;

            // Debug log to verify delta calculations
            Debug.Log($"[Delta Calculation] Frame: {Time.frameCount}\n" +
                     $"Current Frame Position: {currentOptitrackPosition:F6}\n" +
                     $"Last Frame Position: {lastOptitrackPosition.Value:F6}\n" +
                     $"Raw Delta: {(currentOptitrackPosition - lastOptitrackPosition.Value):F6}\n" +
                     $"Scaled Delta: {delta:F6}\n" +
                     $"Accumulated Position: {accumulatedPosition:F6}");

            // Store the current position for next frame's delta calculation
            lastOptitrackPosition = currentOptitrackPosition;
            Debug.Log($"[Transform Update] Frame: {Time.frameCount}, Final Transform Position: {transform.localPosition}");
        }
    }
}