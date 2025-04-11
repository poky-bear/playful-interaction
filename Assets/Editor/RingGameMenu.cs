using UnityEngine;
using UnityEditor;

public class RingGameMenu : MonoBehaviour
{
    [MenuItem("GameObject/Setup Ring Game")]
    static void SetupRingGame()
    {
        // Find or create RingGameSetup
        RingGameSetup setup = FindObjectOfType<RingGameSetup>();
        if (setup == null)
        {
            GameObject setupObj = new GameObject("RingGameSetup");
            setup = setupObj.AddComponent<RingGameSetup>();
        }
        
        // Call setup method
        setup.SetupRingGame();
        
        // Select the created game object
        Selection.activeGameObject = setup.gameObject;
        
        Debug.Log("Ring Game setup complete. You can now connect your ESP32C3 pressure sensor.");
    }
}