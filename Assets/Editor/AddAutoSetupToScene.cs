using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AddAutoSetupToScene
{
    static AddAutoSetupToScene()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        // Check if this is the Obstacles scene
        if (scene.name == "Obstacles")
        {
            // Find the KeyboardSphere
            GameObject keyboardSphere = GameObject.Find("KeyboardSphere");
            if (keyboardSphere == null)
            {
                Debug.LogWarning("KeyboardSphere not found in the Obstacles scene!");
                return;
            }
            
            // Check if the KeyboardController is already attached
            KeyboardController controller = keyboardSphere.GetComponent<KeyboardController>();
            if (controller == null)
            {
                // Create a setup GameObject if it doesn't exist
                GameObject setupObject = GameObject.Find("AutoSetup");
                if (setupObject == null)
                {
                    setupObject = new GameObject("AutoSetup");
                    setupObject.AddComponent<AutoSetupKeyboardController>();
                    Debug.Log("Added AutoSetupKeyboardController to the scene");
                    
                    // Mark the scene as dirty so the user can save the changes
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }
    }
    
    [MenuItem("GameObject/Auto Setup Keyboard Controller")]
    static void AddAutoSetupToActiveScene()
    {
        // Create a setup GameObject if it doesn't exist
        GameObject setupObject = GameObject.Find("AutoSetup");
        if (setupObject == null)
        {
            setupObject = new GameObject("AutoSetup");
            setupObject.AddComponent<AutoSetupKeyboardController>();
            Debug.Log("Added AutoSetupKeyboardController to the scene");
            
            // Mark the scene as dirty so the user can save the changes
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}