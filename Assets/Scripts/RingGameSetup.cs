using UnityEngine;
using UnityEngine.UI;

public class RingGameSetup : MonoBehaviour
{
    [SerializeField] private GameObject ringGamePrefab;
    
    // Call this method to create a ring game UI in the scene
    public void SetupRingGame()
    {
        // Check if there's already a canvas in the scene
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Create a new canvas
            GameObject canvasObj = new GameObject("RingGameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add canvas scaler
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add graphic raycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create ring game UI if prefab is assigned
        if (ringGamePrefab != null)
        {
            Instantiate(ringGamePrefab, canvas.transform);
        }
        else
        {
            // Create ring game UI from scratch
            CreateRingGameUI(canvas.transform);
        }
    }
    
    private void CreateRingGameUI(Transform parent)
    {
        // Create main container
        GameObject container = new GameObject("RingGameContainer");
        container.transform.SetParent(parent, false);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.sizeDelta = new Vector2(600, 600);
        
        // Create outer ring
        GameObject outerRing = CreateUICircle("OuterRing", container.transform, 500, Color.gray);
        
        // Create target zone
        GameObject targetZone = CreateUICircle("TargetZone", container.transform, 480, new Color(0, 1, 0, 0.3f));
        RectTransform targetRect = targetZone.GetComponent<RectTransform>();
        targetRect.sizeDelta = new Vector2(60, 480);
        
        // Create inner ring
        GameObject innerRing = CreateUICircle("InnerRing", container.transform, 460, Color.white);
        
        // Create score text
        GameObject scoreTextObj = new GameObject("ScoreText");
        scoreTextObj.transform.SetParent(container.transform, false);
        RectTransform scoreRect = scoreTextObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1);
        scoreRect.anchorMax = new Vector2(0.5f, 1);
        scoreRect.anchoredPosition = new Vector2(0, 50);
        scoreRect.sizeDelta = new Vector2(300, 50);
        Text scoreText = scoreTextObj.AddComponent<Text>();
        scoreText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        scoreText.fontSize = 36;
        scoreText.alignment = TextAnchor.MiddleCenter;
        scoreText.text = "Score: 0";
        
        // Create feedback text
        GameObject feedbackTextObj = new GameObject("FeedbackText");
        feedbackTextObj.transform.SetParent(container.transform, false);
        RectTransform feedbackRect = feedbackTextObj.AddComponent<RectTransform>();
        feedbackRect.anchorMin = new Vector2(0.5f, 0);
        feedbackRect.anchorMax = new Vector2(0.5f, 0);
        feedbackRect.anchoredPosition = new Vector2(0, -50);
        feedbackRect.sizeDelta = new Vector2(400, 50);
        Text feedbackText = feedbackTextObj.AddComponent<Text>();
        feedbackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        feedbackText.fontSize = 28;
        feedbackText.alignment = TextAnchor.MiddleCenter;
        feedbackText.text = "Apply pressure to stop the ring!";
        
        // Add PressureGameController component
        PressureGameController controller = container.AddComponent<PressureGameController>();
        controller.outerRing = outerRing.GetComponent<Image>();
        controller.innerRing = innerRing.GetComponent<Image>();
        controller.targetZone = targetZone.GetComponent<Image>();
        controller.scoreText = scoreText;
        controller.feedbackText = feedbackText;
        
        // Add ESP32BluetoothManager if not already in the scene
        ESP32BluetoothManager bluetoothManager = FindObjectOfType<ESP32BluetoothManager>();
        if (bluetoothManager == null)
        {
            GameObject bluetoothObj = new GameObject("BluetoothManager");
            bluetoothManager = bluetoothObj.AddComponent<ESP32BluetoothManager>();
        }
        
        // Assign bluetooth manager to controller
        controller.bluetoothManager = bluetoothManager;
    }
    
    private GameObject CreateUICircle(string name, Transform parent, float size, Color color)
    {
        GameObject circle = new GameObject(name);
        circle.transform.SetParent(parent, false);
        
        RectTransform rectTransform = circle.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(size, size);
        
        Image image = circle.AddComponent<Image>();
        image.color = color;
        
        return circle;
    }
}