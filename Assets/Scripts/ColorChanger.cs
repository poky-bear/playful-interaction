using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ColorChanger : MonoBehaviour
{
    private UDPManager udpManager;
    private MeshRenderer meshRenderer;
    private string lastMessage = "";

    void Start()
    {
        // Get reference to the UDPManager
        udpManager = FindObjectOfType<UDPManager>();
        if (udpManager == null)
        {
            Debug.LogError("No UDPManager found in the scene!");
        }

        // Get the MeshRenderer component
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        string currentMessage = udpManager.GetLastReceivedMessage();
        
        // Only process if we have a new message
        if (currentMessage != "" && currentMessage != lastMessage)
        {
            lastMessage = currentMessage;
            
            if (currentMessage.ToLower().Contains("hello"))
            {
                ChangeToRandomColor();
            }
        }
    }

    void ChangeToRandomColor()
    {
        Color randomColor = new Color(
            Random.value, // R
            Random.value, // G
            Random.value, // B
            1.0f        // A
        );

        meshRenderer.material.color = randomColor;
    }
}