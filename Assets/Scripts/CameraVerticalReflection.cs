using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraVerticalReflection : MonoBehaviour
{
    private Camera cam;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        cam = GetComponent<Camera>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Create a reflection matrix
        Matrix4x4 reflectionMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1, 1, 1));
        cam.worldToCameraMatrix = cam.worldToCameraMatrix * reflectionMatrix;
    }

    void LateUpdate()
    {
        // Ensure camera stays in its original position and rotation
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}