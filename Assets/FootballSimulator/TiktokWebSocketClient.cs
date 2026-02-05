using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

#if !UNITY_WEBGL || UNITY_EDITOR
using NativeWebSocket;
#endif

/// <summary>
/// WebSocket Client để kết nối với TikTok Live server và nhận events
/// Hỗ trợ 3 loại events:
/// 1. Like Event (Tim live) → Add vào mảng tap tim
/// 2. Rose Gift → Trigger Super Kick + hiển thị tên
/// 3. Perfume Gift → Trigger Call5Enemy (không hiển thị tên)
/// </summary>
public class TiktokWebSocketClient : MonoBehaviour
{
    [Header("WebSocket Settings")]
    [Tooltip("URL của WebSocket server (ví dụ: ws://localhost:8080)")]
    public string serverUrl = "ws://localhost:8080";
    
    [Tooltip("Tự động kết nối khi Start()")]
    public bool autoConnect = true;
    
    [Tooltip("Thời gian chờ trước khi reconnect (giây)")]
    public float reconnectDelay = 5f;
    
    [Tooltip("Số lần thử reconnect tối đa (-1 = vô hạn)")]
    public int maxReconnectAttempts = -1;
    
    [Header("Gift IDs (TikTok Gift Identifiers)")]
    [Tooltip("ID hoặc tên của quà Rose trong TikTok")]
    public string roseGiftIdentifier = "Rose";
    
    [Tooltip("ID hoặc tên của quà Perfume trong TikTok")]
    public string perfumeGiftIdentifier = "Perfume";
    
    [Header("Debug")]
    [Tooltip("Hiển thị log chi tiết")]
    public bool showDebugLogs = true;
    
    [Tooltip("Test mode: Cho phép gửi fake events bằng UI")]
    public bool testMode = false;
    
    [Header("Status (Read Only)")]
    [SerializeField] private bool isConnected = false;
    [SerializeField] private int reconnectAttempts = 0;
    [SerializeField] private string lastError = "";
    
    // References
    private TiktokReceiver tiktokReceiver;
    private TiktokHeartManager heartManager;
    private TiktokReceiverTest receiverTest;
    
#if !UNITY_WEBGL || UNITY_EDITOR
    private WebSocket websocket;
#endif
    
    private bool isReconnecting = false;
    
    // ===== LIFECYCLE =====
    
    void Start()
    {
        Debug.Log("[TiktokWebSocketClient] ===== INITIALIZING =====");
        
        // Tìm references
        FindReferences();
        
        // Auto connect nếu được bật
        if (autoConnect)
        {
            Connect();
        }
        
        Debug.Log("[TiktokWebSocketClient] ✅ Ready! Use Connect() to start receiving TikTok events.");
    }
    
    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        // Dispatch WebSocket messages trên main thread
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
#endif
    }
    
    void OnDestroy()
    {
        Disconnect();
    }
    
    void OnApplicationQuit()
    {
        Disconnect();
    }
    
    // ===== REFERENCES =====
    
    private void FindReferences()
    {
        tiktokReceiver = FindObjectOfType<TiktokReceiver>();
        heartManager = FindObjectOfType<TiktokHeartManager>();
        receiverTest = FindObjectOfType<TiktokReceiverTest>();
        
        if (tiktokReceiver == null)
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ TiktokReceiver not found!");
        }
        else if (showDebugLogs)
        {
            Debug.Log("[TiktokWebSocketClient] ✅ TiktokReceiver found");
        }
        
        if (heartManager == null)
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ TiktokHeartManager not found!");
        }
        else if (showDebugLogs)
        {
            Debug.Log("[TiktokWebSocketClient] ✅ TiktokHeartManager found");
        }
        
        if (receiverTest == null)
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ TiktokReceiverTest not found!");
        }
        else if (showDebugLogs)
        {
            Debug.Log("[TiktokWebSocketClient] ✅ TiktokReceiverTest found");
        }
    }
    
    // ===== CONNECTION =====
    
    public async void Connect()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (isConnected)
        {
            Debug.LogWarning("[TiktokWebSocketClient] Already connected!");
            return;
        }
        
        Debug.Log($"[TiktokWebSocketClient] Connecting to {serverUrl}...");
        
        try
        {
            websocket = new WebSocket(serverUrl);
            
            // Setup event handlers
            websocket.OnOpen += OnWebSocketOpen;
            websocket.OnMessage += OnWebSocketMessage;
            websocket.OnError += OnWebSocketError;
            websocket.OnClose += OnWebSocketClose;
            
            // Connect
            await websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"[TiktokWebSocketClient] ❌ Connection failed: {e.Message}");
            lastError = e.Message;
            
            // Thử reconnect
            if (!isReconnecting && (maxReconnectAttempts == -1 || reconnectAttempts < maxReconnectAttempts))
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }
#else
        Debug.LogError("[TiktokWebSocketClient] WebSocket not supported in WebGL build!");
#endif
    }
    
    public async void Disconnect()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            Debug.Log("[TiktokWebSocketClient] Disconnecting...");
            await websocket.Close();
            websocket = null;
        }
        
        isConnected = false;
        isReconnecting = false;
#endif
    }
    
    private IEnumerator ReconnectCoroutine()
    {
        isReconnecting = true;
        reconnectAttempts++;
        
        Debug.Log($"[TiktokWebSocketClient] 🔄 Reconnecting in {reconnectDelay} seconds... (Attempt {reconnectAttempts})");
        
        yield return new WaitForSeconds(reconnectDelay);
        
        isReconnecting = false;
        Connect();
    }
    
    // ===== WEBSOCKET EVENT HANDLERS =====
    
    private void OnWebSocketOpen()
    {
        isConnected = true;
        reconnectAttempts = 0;
        lastError = "";
        
        Debug.Log("[TiktokWebSocketClient] ✅ Connected to TikTok server!");
        Debug.Log("[TiktokWebSocketClient] 🎯 Ready to receive events: Like, Rose Gift, Perfume Gift");
    }
    
    private void OnWebSocketMessage(byte[] data)
    {
        string message = Encoding.UTF8.GetString(data);
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokWebSocketClient] 📨 Received: {message}");
        }
        
        // Parse và xử lý message
        ParseAndHandleEvent(message);
    }
    
    private void OnWebSocketError(string error)
    {
        lastError = error;
        Debug.LogError($"[TiktokWebSocketClient] ❌ WebSocket Error: {error}");
    }
    
    private void OnWebSocketClose(WebSocketCloseCode closeCode)
    {
        isConnected = false;
        Debug.Log($"[TiktokWebSocketClient] ⚠️ Connection closed: {closeCode}");
        
        // Auto reconnect nếu không phải do user ngắt
        if (closeCode != WebSocketCloseCode.Normal && !isReconnecting)
        {
            if (maxReconnectAttempts == -1 || reconnectAttempts < maxReconnectAttempts)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }
    }
    
    // ===== MESSAGE PARSING =====
    
    private void ParseAndHandleEvent(string jsonMessage)
    {
        try
        {
            // Parse JSON sử dụng Unity's JsonUtility
            TikTokEvent eventData = JsonUtility.FromJson<TikTokEvent>(jsonMessage);
            
            if (eventData == null)
            {
                Debug.LogWarning($"[TiktokWebSocketClient] ⚠️ Failed to parse event: {jsonMessage}");
                return;
            }
            
            // Xử lý theo loại event
            switch (eventData.type.ToLower())
            {
                case "like":
                    HandleLikeEvent(eventData.userName);
                    break;
                    
                case "gift":
                    int repeatCount = eventData.repeatCount > 0 ? eventData.repeatCount : 1;
                    HandleGiftEvent(eventData.userName, eventData.giftName, repeatCount);
                    break;
                    
                default:
                    if (showDebugLogs)
                    {
                        Debug.Log($"[TiktokWebSocketClient] ℹ️ Unknown event type: {eventData.type}");
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[TiktokWebSocketClient] ❌ Error parsing message: {e.Message}");
            Debug.LogError($"[TiktokWebSocketClient] Raw message: {jsonMessage}");
        }
    }
    
    // ===== GAME EVENT HANDLERS =====
    
    /// <summary>
    /// Xử lý Like event từ TikTok
    /// Thêm user vào mảng tap tim
    /// </summary>
    private void HandleLikeEvent(string userName)
    {
        if (string.IsNullOrEmpty(userName))
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ Like event received but userName is empty!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[TiktokWebSocketClient] 💖 LIKE from {userName}");
        }
        
        // Thêm vào mảng tap tim
        if (heartManager != null)
        {
            heartManager.AddHeartTap(userName);
        }
        else
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ Cannot add heart tap - HeartManager is null!");
        }
    }
    
    /// <summary>
    /// Xử lý Gift event từ TikTok
    /// Phân loại Rose hoặc Perfume
    /// </summary>
    private void HandleGiftEvent(string userName, string giftName, int repeatCount = 1)
    {
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(giftName))
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ Gift event incomplete!");
            return;
        }
        
        // Kiểm tra loại quà
        if (giftName.Equals(roseGiftIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            HandleRoseGift(userName, repeatCount);
        }
        else if (giftName.Equals(perfumeGiftIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            HandlePerfumeGift(userName);
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.Log($"[TiktokWebSocketClient] 🎁 Gift '{giftName}' from {userName} (not handled)");
            }
        }
    }
    
    /// <summary>
    /// Xử lý Rose Gift
    /// → Add vào queue Super Kick (combo = số lần add)
    /// </summary>
    private void HandleRoseGift(string userName, int repeatCount = 1)
    {
        if (repeatCount > 1)
        {
            Debug.Log($"[TiktokWebSocketClient] 🌹 ROSE GIFT x{repeatCount} from {userName}!");
        }
        else
        {
            Debug.Log($"[TiktokWebSocketClient] 🌹 ROSE GIFT from {userName}!");
        }
        
        // Add vào queue (combo count = số lần add)
        if (heartManager != null)
        {
            heartManager.AddToSuperKickQueue(userName, repeatCount);
            Debug.Log($"[TiktokWebSocketClient] ✅ Added {userName} x{repeatCount} to Super Kick queue!");
        }
        else
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ Cannot add to queue - HeartManager is null!");
        }
    }
    
    /// <summary>
    /// Xử lý Perfume Gift
    /// → Trigger Call5Enemy (không hiển thị tên)
    /// </summary>
    private void HandlePerfumeGift(string userName)
    {
        Debug.Log($"[TiktokWebSocketClient] 💐 PERFUME GIFT received!");
        // Không log userName để giữ ẩn danh
        
        // Trigger Call5Enemy
        if (tiktokReceiver != null)
        {
            tiktokReceiver.TriggerCall5Enemy();
            Debug.Log($"[TiktokWebSocketClient] ⚡ Call 5 Enemy activated!");
        }
        else
        {
            Debug.LogWarning("[TiktokWebSocketClient] ⚠️ Cannot trigger Call5Enemy - TiktokReceiver is null!");
        }
    }
    
    // ===== PUBLIC API =====
    
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public string GetStatus()
    {
        if (isConnected) return "Connected";
        if (isReconnecting) return $"Reconnecting... ({reconnectAttempts})";
        if (!string.IsNullOrEmpty(lastError)) return $"Error: {lastError}";
        return "Disconnected";
    }
    
    // ===== TEST METHODS =====
    
    /// <summary>
    /// Simulate Like event cho testing (không cần server)
    /// </summary>
    public void SimulateLikeEvent(string userName)
    {
        if (testMode || Application.isEditor)
        {
            Debug.Log($"[TiktokWebSocketClient] 🧪 TEST: Simulating Like from {userName}");
            HandleLikeEvent(userName);
        }
    }
    
    /// <summary>
    /// Simulate Rose Gift cho testing
    /// </summary>
    public void SimulateRoseGift(string userName, int repeatCount = 1)
    {
        if (testMode || Application.isEditor)
        {
            Debug.Log($"[TiktokWebSocketClient] 🧪 TEST: Simulating Rose Gift x{repeatCount} from {userName}");
            HandleRoseGift(userName, repeatCount);
        }
    }
    
    /// <summary>
    /// Simulate Perfume Gift cho testing
    /// </summary>
    public void SimulatePerfumeGift(string userName = "Anonymous")
    {
        if (testMode || Application.isEditor)
        {
            Debug.Log($"[TiktokWebSocketClient] 🧪 TEST: Simulating Perfume Gift");
            HandlePerfumeGift(userName);
        }
    }
}

// ===== DATA CLASSES =====

/// <summary>
/// Class để deserialize JSON từ TikTok server
/// Format:
/// {
///   "type": "like" | "gift",
///   "userName": "user123",
///   "giftName": "Rose" | "Perfume" (chỉ khi type = gift)
/// }
/// </summary>
[System.Serializable]
public class TikTokEvent
{
    public string type;      // "like" hoặc "gift"
    public string userName;  // Tên user TikTok
    public string giftName;  // Tên quà (nếu type = gift)
    public int giftId;       // ID quà (optional)
    public int likeCount;    // Số like (optional)
    public int repeatCount;  // Combo count cho gift (optional, default = 1)
}
