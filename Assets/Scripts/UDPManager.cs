using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDPManager : MonoBehaviour
{
    [Header("UDP Settings")]
    [Tooltip("IP address of this Unity application")]
    public string receiverIP = "192.168.1.2"; // Should match Arduino's udpAddress
    [Tooltip("Port this Unity application listens on")]
    public int receiverPort = 5006; // Should match Arduino's udpSenderPort
    [Tooltip("Port this Unity application sends to")]
    public int senderPort = 5005; // Should match Arduino's udpReceiverPort

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool threadRunning = false;
    private string lastReceivedMessage = "";
    private object messageLock = new object();

    void Start()
    {
        InitializeUDP();
    }

    void InitializeUDP()
    {
        try
        {
            // Create UDP client and bind to the receiver port
            udpClient = new UdpClient(receiverPort);
            Debug.Log($"UDP initialized. Listening on port {receiverPort}");

            // Start receive thread
            threadRunning = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"UDP initialization failed: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (threadRunning)
        {
            try
            {
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);

                lock (messageLock)
                {
                    lastReceivedMessage = message;
                }

                // Log in thread-safe way
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Debug.Log($"Received: {message}");
                });

                // You can automatically send a response if needed
                SendMessage("Message received by Unity");
            }
            catch (Exception e)
            {
                if (threadRunning) // Only log if thread should be running
                {
                    Debug.LogError($"Error receiving UDP data: {e.Message}");
                }
            }
        }
    }

    public void SendMessage(string message)
    {
        if (udpClient == null) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, receiverIP, senderPort);
            Debug.Log($"Sent: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending UDP message: {e.Message}");
        }
    }

    public string GetLastReceivedMessage()
    {
        lock (messageLock)
        {
            return lastReceivedMessage;
        }
    }

    void OnDestroy()
    {
        threadRunning = false;
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }

        if (receiveThread != null)
        {
            receiveThread.Abort();
            receiveThread = null;
        }
    }
}

// Helper class to run code on Unity main thread
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher instance;
    private Queue<Action> queue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UnityMainThreadDispatcher>();
                if (instance == null)
                {
                    GameObject go = new GameObject("UnityMainThreadDispatcher");
                    instance = go.AddComponent<UnityMainThreadDispatcher>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    public void Enqueue(Action action)
    {
        lock (queue)
        {
            queue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (queue)
        {
            while (queue.Count > 0)
            {
                queue.Dequeue().Invoke();
            }
        }
    }
}