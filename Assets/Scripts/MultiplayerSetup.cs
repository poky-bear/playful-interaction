using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

// Add a menu item to set up the multiplayer mode
public class MultiplayerSetup : MonoBehaviour
{
    [MenuItem("GameObject/Setup Multiplayer Mode")]
    static void SetupMultiplayerMode()
    {
        // Create a GameObject for the multiplayer controller
        GameObject multiplayerObject = new GameObject("MultiplayerController");
        
        // Add the multiplayer script
        MultiplayerRingGame multiplayerGame = multiplayerObject.AddComponent<MultiplayerRingGame>();
        
        // Find player spheres
        GameObject player1Sphere = GameObject.Find("Sphere");
        GameObject player2Sphere = GameObject.Find("Player2Sphere");
        
        if (player1Sphere == null)
        {
            Debug.LogError("Player 1 sphere not found! Make sure you have a GameObject named 'Sphere' in your scene.");
            return;
        }
        
        if (player2Sphere == null)
        {
            Debug.LogError("Player 2 sphere not found! Make sure you have set up Player 2 first (GameObject > Setup Player 2).");
            return;
        }
        
        // Assign references
        multiplayerGame.player1Sphere = player1Sphere;
        multiplayerGame.player2Sphere = player2Sphere;
        
        // Create UI for multiplayer mode
        CreateMultiplayerUI(multiplayerGame);
        
        Debug.Log("Multiplayer mode setup complete!");
    }
    
    static void CreateMultiplayerUI(MultiplayerRingGame gameController)
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
        
        // Create a panel for Multiplayer UI
        GameObject multiplayerPanel = new GameObject("MultiplayerPanel");
        multiplayerPanel.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = multiplayerPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0.3f);
        panelRect.offsetMin = new Vector2(0, 0);
        panelRect.offsetMax = new Vector2(0, 0);
        
        // Add MultiplayerRingGameUI component
        MultiplayerRingGameUI ui = multiplayerPanel.AddComponent<MultiplayerRingGameUI>();
        ui.gameController = gameController;
        
        // Create instructions text
        GameObject instructionsObj = new GameObject("MultiplayerInstructions");
        instructionsObj.transform.SetParent(multiplayerPanel.transform, false);
        Text instructionsText = instructionsObj.AddComponent<Text>();
        instructionsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionsText.fontSize = 16;
        instructionsText.color = Color.white;
        instructionsText.alignment = TextAnchor.UpperLeft;
        
        RectTransform instructionsRect = instructionsObj.GetComponent<RectTransform>();
        instructionsRect.anchorMin = new Vector2(0, 0.7f);
        instructionsRect.anchorMax = new Vector2(1, 1);
        instructionsRect.offsetMin = new Vector2(10, 0);
        instructionsRect.offsetMax = new Vector2(-10, -10);
        
        // Create status text
        GameObject statusObj = new GameObject("MultiplayerStatus");
        statusObj.transform.SetParent(multiplayerPanel.transform, false);
        Text statusText = statusObj.AddComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 20;
        statusText.color = Color.white;
        statusText.alignment = TextAnchor.UpperLeft;
        
        RectTransform statusRect = statusObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0.4f);
        statusRect.anchorMax = new Vector2(1, 0.7f);
        statusRect.offsetMin = new Vector2(10, 0);
        statusRect.offsetMax = new Vector2(-10, 0);
        
        // Create feedback text
        GameObject feedbackObj = new GameObject("MultiplayerFeedback");
        feedbackObj.transform.SetParent(multiplayerPanel.transform, false);
        Text feedbackText = feedbackObj.AddComponent<Text>();
        feedbackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        feedbackText.fontSize = 24;
        feedbackText.color = Color.green;
        feedbackText.alignment = TextAnchor.UpperLeft;
        
        RectTransform feedbackRect = feedbackObj.GetComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0, 0.1f);
        feedbackRect.anchorMax = new Vector2(1, 0.4f);
        feedbackRect.offsetMin = new Vector2(10, 0);
        feedbackRect.offsetMax = new Vector2(-10, 0);
        
        // Assign the text components to the UI
        ui.instructionsText = instructionsText;
        ui.statusText = statusText;
        ui.feedbackText = feedbackText;
    }
}
#endif