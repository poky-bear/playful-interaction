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

    [Tooltip("The X coordinate of the room's center")]
    public float RoomCenterX = -3.37f;

    [Tooltip("The Z coordinate of the room's center")]
    public float RoomCenterZ = -0.83f;

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
            Vector3 mirroredPosition = currentOptitrackPosition;
    
            // Calculate position relative to center, then mirror it
            float relativeX = currentOptitrackPosition.x - RoomCenterX;
            float relativeZ = currentOptitrackPosition.z - RoomCenterZ;

            // Mirror the relative positions
            mirroredPosition.x = RoomCenterX - relativeX;
            mirroredPosition.z = RoomCenterZ - relativeZ;

            // Update the transform with mirrored position
            transform.localPosition = mirroredPosition;
            transform.localRotation = rbState.Pose.Orientation;

            // Debug log to verify mirroring
            Debug.Log($"[Position Update] Frame: {Time.frameCount}\n" +
                     $"Original Position: {currentOptitrackPosition:F3}\n" +
                     $"Relative to Center: ({relativeX:F3}, {currentOptitrackPosition.y:F3}, {relativeZ:F3})\n" +
                     $"Mirrored Position: {mirroredPosition:F3}");

        }
    }
}