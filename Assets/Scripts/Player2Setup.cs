using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

// Add a menu item to set up the second player
public class Player2Setup : MonoBehaviour
{
    [MenuItem("GameObject/Setup Player 2")]
    static void SetupPlayer2()
    {
        // Create a new sphere for Player 2
        GameObject player2Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player2Sphere.name = "Player2Sphere";
        
        // Position it away from the first player
        player2Sphere.transform.position = new Vector3(5f, 0f, 0f);
        
        // Add required components
        player2Sphere.AddComponent<ConcentricRings>();
        player2Sphere.AddComponent<Player2RingGameController>();
        Player2Controller controller = player2Sphere.AddComponent<Player2Controller>();
        
        // Set up the default movement keys (WASD)
        controller.moveLeftKey = KeyCode.A;
        controller.moveRightKey = KeyCode.D;
        controller.moveForwardKey = KeyCode.W;
        controller.moveBackwardKey = KeyCode.S;
        
        // Set up the ConcentricRings component
        ConcentricRings rings = player2Sphere.GetComponent<ConcentricRings>();
        rings.targetSphere = player2Sphere;
        
        // Create a cube for Player 2
        GameObject player2Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player2Cube.name = "Player2Cube";
        player2Cube.transform.position = new Vector3(0, -10, 0); // Hide it initially
        player2Cube.SetActive(false);
        
        // Set up the Player2RingGameController
        Player2RingGameController controller = player2Sphere.GetComponent<Player2RingGameController>();
        controller.concentricRings = rings;
        controller.cubeObject = player2Cube;
        
        // Create UI for Player 2
        CreatePlayer2UI(controller);
        
        Debug.Log("Player 2 setup complete!");
    }
    
    static void CreatePlayer2UI(Player2RingGameController controller)
    {
        // Check if there's already a canvas in the scene
        Canvas existingCanvas = GameObject.FindObjectOfType<Canvas>();
        GameObject canvasObject;
        
        if (existingCanvas != null)
        {
            canvasObject = existingCanvas.gameObject;
        }
        else
        {
            // Create a new canvas
            canvasObject = new GameObject("Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }
        
        // Create a panel for Player 2 UI
        GameObject player2Panel = new GameObject("Player2Panel");
        player2Panel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = player2Panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = new Vector2(0, 0);
        panelRect.offsetMax = new Vector2(0, 0);
        
        // Add Player2RingGameUI component
        Player2RingGameUI ui = player2Panel.AddComponent<Player2RingGameUI>();
        ui.gameController = controller;
        
        // Create instructions text
        GameObject instructionsObj = new GameObject("Player2Instructions");
        instructionsObj.transform.SetParent(player2Panel.transform, false);
        Text instructionsText = instructionsObj.AddComponent<Text>();
        instructionsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionsText.fontSize = 16;
        instructionsText.color = Color.white;
        instructionsText.alignment = TextAnchor.UpperRight;
        
        RectTransform instructionsRect = instructionsObj.GetComponent<RectTransform>();
        instructionsRect.anchorMin = new Vector2(0, 0.7f);
        instructionsRect.anchorMax = new Vector2(1, 1);
        instructionsRect.offsetMin = new Vector2(10, 0);
        instructionsRect.offsetMax = new Vector2(-10, -10);
        
        // Create status text
        GameObject statusObj = new GameObject("Player2Status");
        statusObj.transform.SetParent(player2Panel.transform, false);
        Text statusText = statusObj.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 20;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperRight;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.5f);
        statusRect.anchorMax = new Vector2(1, 0.7f);
        statusRect.offsetMin = new Vector2(10, 0);
        statusRect.offsetMax = new Vector2(-10, 0);
        
        // Create feedback text
        GameObject feedbackObj = new GameObject("Player2Feedback");
        feedbackObj.transform.SetParent(player2Panel.transform, false);
        Text feedbackText = feedbackObj.AddComponent<Text>();
        feedbackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        feedbackText.fontSize = 24;
        feedbackText.color = Color.green;
        feedbackText.alignment = TextAnchor.UpperRight;
        
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0, 0.3f);
        feedbackRect.anchorMax = new Vector2(1, 0.5f);
        feedbackRect.offsetMin = new Vector2(10, 0);
        feedbackRect.offsetMax = new Vector2(-10, 0);
        
        // Assign the text components to the UI
        ui.instructionsText = instructionsText;
        ui.statusText = statusText;
        ui.feedbackText = feedbackText;
    }
}
#endif