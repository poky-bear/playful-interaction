using UnityEngine;

public class MultiCameraManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraSetup
    {
        public Camera camera;
        public int targetDisplay = 0;
    }

    public CameraSetup[] cameraSetups = new CameraSetup[2];

    private void Start()
    {
        // Log display information
        Debug.Log($"Number of displays connected: {Display.displays.Length}");
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Debug.Log($"Display {i}: {Display.displays[i].systemWidth}x{Display.displays[i].systemHeight}");
        }
        
        // Activate all displays
        for (int i = 1; i < Display.displays.Length; i++)  // Start from 1 since main display is already active
        {
            Display.displays[i].Activate();
            Debug.Log($"Activated display {i}");
        }

        // Configure cameras
        for (int i = 0; i < cameraSetups.Length; i++)
        {
            if (cameraSetups[i].camera != null)
            {
                cameraSetups[i].camera.targetDisplay = cameraSetups[i].targetDisplay;
                Debug.Log($"Assigned Camera {i} to display {cameraSetups[i].targetDisplay}");
            }
        }
    }
}