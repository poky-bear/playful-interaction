using UnityEngine;
using UnityEditor;

public class RingGameSetup : Editor
{
    [MenuItem("GameObject/Setup Ring Game")]
    public static void SetupRingGame()
    {
        // Find or create the main sphere
        GameObject sphere = GameObject.Find("MainSphere");
        if (sphere == null)
        {
            sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "MainSphere";
            sphere.transform.position = Vector3.zero;
        }
        
        // Add ConcentricRings component if not present
        ConcentricRings concentricRings = sphere.GetComponent<ConcentricRings>();
        if (concentricRings == null)
        {
            concentricRings = sphere.AddComponent<ConcentricRings>();
            concentricRings.targetSphere = sphere;
            concentricRings.minDistanceToFirstRing = 1.0f;
            concentricRings.ringSpacing = 1.0f;
            concentricRings.ringThickness = 0.1f;
        }
        
        // Add RingGameController component if not present
        RingGameController gameController = sphere.GetComponent<RingGameController>();
        if (gameController == null)
        {
            gameController = sphere.AddComponent<RingGameController>();
            gameController.concentricRings = concentricRings;
        }
        
        // Add KeyboardController component if not present
        KeyboardController keyboardController = sphere.GetComponent<KeyboardController>();
        if (keyboardController == null)
        {
            keyboardController = sphere.AddComponent<KeyboardController>();
            keyboardController.usePhysics = true;
            keyboardController.moveSpeed = 10f;
            keyboardController.maxSpeed = 15f;
            keyboardController.drag = 0.5f;
            
            // Add Rigidbody if needed
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = sphere.AddComponent<Rigidbody>();
                rb.drag = 0.5f;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
        
        // Create UI Canvas
        GameObject canvas = GameObject.Find("UICanvas");
        if (canvas == null)
        {
            canvas = new GameObject("UICanvas");
            Canvas canvasComponent = canvas.AddComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create instructions text
            GameObject instructionsObj = new GameObject("InstructionsText");
            instructionsObj.transform.SetParent(canvas.transform, false);
            UnityEngine.UI.Text instructionsText = instructionsObj.AddComponent<UnityEngine.UI.Text>();
            instructionsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            instructionsText.fontSize = 16;
            instructionsText.color = Color.white;
            instructionsText.alignment = TextAnchor.UpperLeft;
            instructionsText.rectTransform.anchorMin = new Vector2(0, 1);
            instructionsText.rectTransform.anchorMax = new Vector2(0, 1);
            instructionsText.rectTransform.pivot = new Vector2(0, 1);
            instructionsText.rectTransform.anchoredPosition = new Vector2(10, -10);
            instructionsText.rectTransform.sizeDelta = new Vector2(400, 100);
            
            // Create status text
            GameObject statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(canvas.transform, false);
            UnityEngine.UI.Text statusText = statusObj.AddComponent<UnityEngine.UI.Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            statusText.fontSize = 20;
            statusText.color = Color.white;
            statusText.alignment = TextAnchor.UpperRight;
            statusText.rectTransform.anchorMin = new Vector2(1, 1);
            statusText.rectTransform.anchorMax = new Vector2(1, 1);
            statusText.rectTransform.pivot = new Vector2(1, 1);
            statusText.rectTransform.anchoredPosition = new Vector2(-10, -10);
            statusText.rectTransform.sizeDelta = new Vector2(300, 50);
            
            // Create feedback text (for hit accuracy)
            GameObject feedbackObj = new GameObject("FeedbackText");
            feedbackObj.transform.SetParent(canvas.transform, false);
            UnityEngine.UI.Text feedbackText = feedbackObj.AddComponent<UnityEngine.UI.Text>();
            feedbackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            feedbackText.fontSize = 24;
            feedbackText.fontStyle = FontStyle.Bold;
            feedbackText.color = Color.white;
            feedbackText.alignment = TextAnchor.MiddleCenter;
            feedbackText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            feedbackText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            feedbackText.rectTransform.anchoredPosition = new Vector2(0, 0);
            feedbackText.rectTransform.sizeDelta = new Vector2(400, 100);
            feedbackText.gameObject.SetActive(false); // Start hidden
            
            // Add UI controller
            RingGameUI uiController = canvas.AddComponent<RingGameUI>();
            uiController.instructionsText = instructionsText;
            uiController.statusText = statusText;
            uiController.feedbackText = feedbackText;
            uiController.gameController = gameController;
        }
        
    }
}