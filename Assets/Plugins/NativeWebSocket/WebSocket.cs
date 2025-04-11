using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace NativeWebSocket
{
    public delegate void WebSocketOpenEventHandler();
    public delegate void WebSocketMessageEventHandler(byte[] data);
    public delegate void WebSocketErrorEventHandler(string errorMsg);
    public delegate void WebSocketCloseEventHandler(WebSocketCloseCode closeCode);

    public enum WebSocketCloseCode
    {
        /* Standard close codes */
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    public enum WebSocketState
    {
        Connecting,
        Open,
        Closing,
        Closed
    }

    public class WebSocket
    {
        private Uri uri;
        private Dictionary<string, string> headers;
        private List<string> subprotocols;
        private int instanceId;

        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        public WebSocketState State { get; private set; }

        public WebSocket(string url, Dictionary<string, string> headers = null, List<string> subprotocols = null)
        {
            this.uri = new Uri(url);
            this.headers = headers ?? new Dictionary<string, string>();
            this.subprotocols = subprotocols ?? new List<string>();
            this.State = WebSocketState.Closed;
            this.instanceId = WebSocketManager.Instance.Add(this);
        }

        ~WebSocket()
        {
            WebSocketManager.Instance.Remove(this.instanceId);
        }

        public Task Connect()
        {
            var task = new TaskCompletionSource<bool>();

            if (this.State == WebSocketState.Open)
            {
                task.SetResult(true);
                return task.Task;
            }

            this.State = WebSocketState.Connecting;

            // Simulate connection
            Task.Delay(100).ContinueWith(_ =>
            {
                this.State = WebSocketState.Open;
                this.OnOpen?.Invoke();
                task.SetResult(true);
            });

            return task.Task;
        }

        public Task Close(WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null)
        {
            var task = new TaskCompletionSource<bool>();

            if (this.State == WebSocketState.Closed)
            {
                task.SetResult(true);
                return task.Task;
            }

            this.State = WebSocketState.Closing;

            // Simulate closing
            Task.Delay(100).ContinueWith(_ =>
            {
                this.State = WebSocketState.Closed;
                this.OnClose?.Invoke(code);
                task.SetResult(true);
            });

            return task.Task;
        }

        public Task Send(byte[] data)
        {
            var task = new TaskCompletionSource<bool>();

            if (this.State != WebSocketState.Open)
            {
                task.SetException(new Exception("WebSocket is not open"));
                return task.Task;
            }

            // Simulate sending
            Task.Delay(10).ContinueWith(_ =>
            {
                task.SetResult(true);
            });

            return task.Task;
        }

        public Task Send(string message)
        {
            return Send(System.Text.Encoding.UTF8.GetBytes(message));
        }

        public void DispatchMessageQueue()
        {
            // This is a simulation - in a real implementation, this would process messages from a queue
        }

        // Method to simulate receiving a message (for testing)
        public void SimulateMessage(string message)
        {
            if (this.State == WebSocketState.Open)
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                this.OnMessage?.Invoke(data);
            }
        }

        // Method to simulate an error (for testing)
        public void SimulateError(string errorMsg)
        {
            this.OnError?.Invoke(errorMsg);
        }
    }

    // Simple manager to keep track of WebSocket instances
    internal class WebSocketManager
    {
        private static WebSocketManager instance;
        private Dictionary<int, WebSocket> webSockets;
        private int nextId = 0;

        public static WebSocketManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WebSocketManager();
                }
                return instance;
            }
        }

        private WebSocketManager()
        {
            webSockets = new Dictionary<int, WebSocket>();
        }

        public int Add(WebSocket webSocket)
        {
            int id = nextId++;
            webSockets[id] = webSocket;
            return id;
        }

        public void Remove(int id)
        {
            if (webSockets.ContainsKey(id))
            {
                webSockets.Remove(id);
            }
        }

        public WebSocket Get(int id)
        {
            if (webSockets.TryGetValue(id, out WebSocket webSocket))
            {
                return webSocket;
            }
            return null;
        }
    }
}