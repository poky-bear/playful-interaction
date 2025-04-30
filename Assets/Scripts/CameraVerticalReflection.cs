using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraVerticalReflection : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Flip the camera's view vertically by scaling Y by -1 in the projection matrix
        cam.projectionMatrix = cam.projectionMatrix * Matrix4x4.Scale(new Vector3(1, -1, 1));
    }
}