using UnityEngine;
using UnityEditor;

public class WebSocketSetupMenu : MonoBehaviour
{
    [MenuItem("ESP32/Add WebSocket Manager")]
    static void AddWebSocketManager()
    {
        GameObject webSocketManager = new GameObject("ESP32WebSocketManager");
        webSocketManager.AddComponent<ESP32WebSocketManager>();
        Selection.activeGameObject = webSocketManager;
        Debug.Log("ESP32WebSocketManager added to the scene.");
    }

    [MenuItem("ESP32/Create WebSocket Ring Game")]
    static void CreateWebSocketRingGame()
    {
        // First, make sure we have a WebSocket manager
        ESP32WebSocketManager manager = FindObjectOfType<ESP32WebSocketManager>();
        if (manager == null)
        {
            GameObject webSocketManager = new GameObject("ESP32WebSocketManager");
            manager = webSocketManager.AddComponent<ESP32WebSocketManager>();
            Debug.Log("ESP32WebSocketManager added to the scene.");
        }

        // Create the ring game setup
        GameObject ringGameSetup = new GameObject("WebSocketRingGameSetup");
        WebSocketRingGameSetup setup = ringGameSetup.AddComponent<WebSocketRingGameSetup>();
        setup.SetupRingGame();
        
        Selection.activeGameObject = ringGameSetup;
        Debug.Log("WebSocket Ring Game created.");
    }

    [MenuItem("ESP32/Create WebSocket Demo")]
    static void CreateWebSocketDemo()
    {
        // First, make sure we have a WebSocket manager
        ESP32WebSocketManager manager = FindObjectOfType<ESP32WebSocketManager>();
        if (manager == null)
        {
            GameObject webSocketManager = new GameObject("ESP32WebSocketManager");
            manager = webSocketManager.AddComponent<ESP32WebSocketManager>();
            Debug.Log("ESP32WebSocketManager added to the scene.");
        }

        // Create a canvas if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Debug.Log("Canvas added to the scene.");
        }

        // Create the demo UI
        GameObject demoObj = new GameObject("ESP32WebSocketDemo");
        ESP32WebSocketDemo demo = demoObj.AddComponent<ESP32WebSocketDemo>();
        
        // Create UI elements
        GameObject textObj = new GameObject("PressureText");
        textObj.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = "Pressure: 0";
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, 100);
        textRect.sizeDelta = new Vector2(300, 50);

        GameObject sliderObj = new GameObject("PressureSlider");
        sliderObj.transform.SetParent(canvas.transform, false);
        UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();
        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        sliderRect.anchoredPosition = new Vector2(0, 0);
        sliderRect.sizeDelta = new Vector2(300, 20);

        // Create slider background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        UnityEngine.UI.Image bgImage = bgObj.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = Color.gray;
        RectTransform bgRect = bgImage.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        slider.targetGraphic = bgImage;

        // Create slider fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(sliderObj.transform, false);
        UnityEngine.UI.Image fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = new Vector2(0, 0);
        
        // Create slider handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(sliderObj.transform, false);
        UnityEngine.UI.Image handleImage = handleObj.AddComponent<UnityEngine.UI.Image>();
        handleImage.color = Color.white;
        RectTransform handleRect = handleImage.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 30);
        
        // Setup slider
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 0;
        
        // Create indicator sphere
        GameObject indicatorObj = new GameObject("PressureIndicator");
        indicatorObj.transform.position = new Vector3(0, -2, 0);
        SphereCollider sphereCollider = indicatorObj.AddComponent<SphereCollider>();
        sphereCollider.radius = 0.5f;
        MeshFilter meshFilter = indicatorObj.AddComponent<MeshFilter>();
        meshFilter.mesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        MeshRenderer meshRenderer = indicatorObj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.blue;
        
        // Assign references to the demo script
        demo.pressureText = text;
        demo.pressureSlider = slider;
        demo.pressureIndicator = indicatorObj;
        demo.webSocketManager = manager;
        
        Selection.activeGameObject = demoObj;
        Debug.Log("ESP32WebSocketDemo created.");
    }
}