using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraVerticalReflection : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Just flip the sign of the first column of the projection matrix
        Matrix4x4 mat = cam.projectionMatrix;
        mat.m00 = -mat.m00;
        cam.projectionMatrix = mat;
    }
}