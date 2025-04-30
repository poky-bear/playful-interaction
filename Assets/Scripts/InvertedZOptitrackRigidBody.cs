using UnityEngine;

/// <summary>
/// Extends OptitrackRigidBody to invert the Z-axis movement.
/// </summary>
public class InvertedZOptitrackRigidBody : OptitrackRigidBody
{
    protected void UpdatePose()
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