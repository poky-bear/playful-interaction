using UnityEngine;

public class MultiCameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraSetup
    {
        public Camera camera;
        public GameObject target;
        public Vector3 offset = new Vector3(0, 10, -10);
        public float smoothSpeed = 0.125f;
        public int targetDisplay = 0;
    }

    public CameraSetup[] cameraSetups = new CameraSetup[2];
    public bool isGameMode = false;

    private void Start()
    {
        // Ensure we have the correct number of displays
        Debug.Log($"Number of displays connected: {Display.displays.Length}");
        
        // Activate all displays
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Display.displays[i].Activate();
        }

        // Configure cameras
        for (int i = 0; i < cameraSetups.Length; i++)
        {
            if (cameraSetups[i].camera != null)
            {
                cameraSetups[i].camera.targetDisplay = cameraSetups[i].targetDisplay;
            }
        }
    }

    private void LateUpdate()
    {
        if (!isGameMode) return;

        foreach (var setup in cameraSetups)
        {
            if (setup.camera != null && setup.target != null)
            {
                // Calculate the desired position
                Vector3 desiredPosition = setup.target.transform.position + setup.offset;
                
                // Smoothly move the camera
                Vector3 smoothedPosition = Vector3.Lerp(setup.camera.transform.position, desiredPosition, setup.smoothSpeed);
                setup.camera.transform.position = smoothedPosition;
                
                // Make the camera look at the target
                setup.camera.transform.LookAt(setup.target.transform);
            }
        }
    }

    public void SetGameMode(bool enabled)
    {
        isGameMode = enabled;
    }
}