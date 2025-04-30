using UnityEngine;

/// <summary>
/// Extends OptitrackRigidBody to invert the Z-axis movement.
/// </summary>
public class InvertedZOptitrackRigidBody : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public int RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;

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
            Vector3 position = rbState.Pose.Position;
            position.z = -position.z; // Invert the Z position

            this.transform.localPosition = position;
            this.transform.localRotation = rbState.Pose.Orientation;
        }
    }
}