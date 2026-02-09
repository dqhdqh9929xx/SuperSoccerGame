using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using FStudio.MatchEngine;

/// <summary>
/// Test script cho TiktokReceiver
/// Bấm phím T → Trigger Super Kick x1 (direct, 1 quả)
/// Bấm phím Y → Trigger Call 5 Enemy
/// Bấm phím U → Add Heart (test TikTok viewer)
/// Bấm phím R → Rose Gift x1 (queue, 1 quả)
/// Bấm phím O → Rose Gift x5 Combo (queue, sút ra 5 quả bóng cùng lúc)
/// Bấm phím P → Perfume Gift
/// Bấm phím G → Rose Gift x3 Combo (queue, sút ra 3 quả bóng cùng lúc)
/// </summary>
public class TiktokReceiverTest : MonoBehaviour {
    [Header("Test Keys")]
    [Tooltip("Phím để test Super Kick")]
    public KeyCode superKickKey = KeyCode.T;
    
    [Tooltip("Phím để test Call 5 Enemy")]
    public KeyCode call5EnemyKey = KeyCode.Y;
    
    [Header("References")]
    private TiktokReceiver receiver;
    private TiktokHeartManager heartManager;
    private TiktokWebSocketClient wsClient;
    
    [Header("UI")]
    [Tooltip("Hiện hướng dẫn trên màn hình")]
    public bool showOnScreenInstructions = true;
    
    [Header("TikTok Heart Test")]
    [Tooltip("TextMeshProUGUI để hiển thị số heart từ TikTok viewer")]
    public TextMeshProUGUI textCountHeart;
    public TextMeshProUGUI currentNameSuperKick;
    
    // Danh sách 5 tên test
    private string[] testUsers = new string[]
    {
        "User1_NguyenVanA",
        "User2_TranThiB", 
        "User3_LeVanC",
        "User4_PhamThiD",
        "User5_HoangVanE"
    };
    
    void Start() {
        Debug.Log("[TiktokReceiverTest] ===== STARTING =====");
        
        receiver = FindObjectOfType<TiktokReceiver>();
        heartManager = FindObjectOfType<TiktokHeartManager>();
        wsClient = FindObjectOfType<TiktokWebSocketClient>();
        
        if (receiver == null) {
            Debug.LogError("[TiktokReceiverTest] ❌ TiktokReceiver NOT FOUND in scene!");
            Debug.LogError("[TiktokReceiverTest] Please create a GameObject with TiktokReceiver component!");
        } else {
            Debug.Log($"[TiktokReceiverTest] ✅ TiktokReceiver FOUND!");
        }
        
        if (heartManager == null) {
            Debug.LogError("[TiktokReceiverTest] ❌ TiktokHeartManager NOT FOUND in scene!");
            Debug.LogError("[TiktokReceiverTest] Please create a GameObject with TiktokHeartManager component!");
        } else {
            Debug.Log($"[TiktokReceiverTest] ✅ TiktokHeartManager FOUND!");
        }
        
        if (wsClient == null) {
            Debug.LogWarning("[TiktokReceiverTest] ⚠️ TiktokWebSocketClient NOT FOUND (Rose/Perfume test disabled)");
        } else {
            Debug.Log($"[TiktokReceiverTest] ✅ TiktokWebSocketClient FOUND!");
        }
        
        Debug.Log($"[TiktokReceiverTest] ✅ Ready! Press {superKickKey} for Super Kick, {call5EnemyKey} for Call 5 Enemy");
        Debug.Log("[TiktokReceiverTest] Test users:");
        for (int i = 0; i < testUsers.Length; i++)
        {
            Debug.Log($"  {i + 1}. {testUsers[i]}");
        }
    }
    
    void Update() {
        // Check keyboard
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Check nếu receiver null thì cố tìm lại
        if (receiver == null) {
            receiver = FindObjectOfType<TiktokReceiver>();
            if (receiver != null) {
                Debug.Log("[TiktokReceiverTest] ✅ TiktokReceiver found in Update!");
            }
        }
        
        // Check nếu heartManager null thì cố tìm lại
        if (heartManager == null) {
            heartManager = FindObjectOfType<TiktokHeartManager>();
            if (heartManager != null) {
                Debug.Log("[TiktokReceiverTest] ✅ TiktokHeartManager found in Update!");
            }
        }
        
        // Check nếu wsClient null thì cố tìm lại
        if (wsClient == null) {
            wsClient = FindObjectOfType<TiktokWebSocketClient>();
            if (wsClient != null) {
                Debug.Log("[TiktokReceiverTest] ✅ TiktokWebSocketClient found in Update!");
            }
        }
        
        // Test Super Kick (T key)
        if (keyboard.tKey.wasPressedThisFrame) {
            Debug.Log($"[TiktokReceiverTest] ⚡ KEY PRESSED: T");
            Debug.Log($"[TiktokReceiverTest] ⚡ Triggering Super Kick...");
            receiver.TriggerSuperKick();
        }
        
        // Test Call 5 Enemy (Y key)
        if (keyboard.yKey.wasPressedThisFrame) {
            Debug.Log($"[TiktokReceiverTest] ⚡ KEY PRESSED: Y");
            Debug.Log($"[TiktokReceiverTest] ⚡ Triggering Call 5 Enemy...");
            receiver.TriggerCall5Enemy();
        }
        
        // Test TikTok Heart System (U key để simulate heart từ random user)
        if (keyboard.uKey.wasPressedThisFrame) {
            if (heartManager != null)
            {
                // Random chọn 1 trong 5 user
                int randomUserIndex = Random.Range(0, testUsers.Length);
                string selectedUser = testUsers[randomUserIndex];
                
                // Add heart tap
                heartManager.AddHeartTap(selectedUser);
                
                Debug.Log($"[TiktokReceiverTest] 💖 KEY PRESSED: U → Random user: {selectedUser}");
                
                // Cập nhật UI text nếu có
                if (textCountHeart != null) {
                    textCountHeart.text = heartManager.GetCurrentHeartCount().ToString();
                }
            }
            else
            {
                Debug.LogWarning("[TiktokReceiverTest] HeartManager is null!");
            }
        }
        
        // Test Rose Gift x1 (R key)
        if (keyboard.rKey.wasPressedThisFrame) {
            if (wsClient != null)
            {
                // Random chọn 1 trong 5 user
                int randomUserIndex = Random.Range(0, testUsers.Length);
                string selectedUser = testUsers[randomUserIndex];
                
                // Simulate Rose Gift x1
                wsClient.SimulateRoseGift(selectedUser, 1);
                
                Debug.Log($"[TiktokReceiverTest] 🌹 KEY PRESSED: R → Rose Gift x1 from {selectedUser}");
            }
            else
            {
                Debug.LogWarning("[TiktokReceiverTest] WebSocketClient is null!");
            }
        }
        
        // Test Rose Gift x5 Combo (O key) → sút ra 5 quả bóng cùng lúc
        if (keyboard.oKey.wasPressedThisFrame) {
            if (wsClient != null)
            {
                // Random chọn 1 trong 5 user
                int randomUserIndex = Random.Range(0, testUsers.Length);
                string selectedUser = testUsers[randomUserIndex];
                
                // Simulate Rose Gift x5 (combo) → 1 entry trong queue, sút ra 5 quả
                wsClient.SimulateRoseGift(selectedUser, 5);
                
                Debug.Log($"[TiktokReceiverTest] 🌹x5 KEY PRESSED: O → Rose Gift x5 COMBO from {selectedUser} (will shoot 5 balls at once!)");
            }
            else
            {
                Debug.LogWarning("[TiktokReceiverTest] WebSocketClient is null!");
            }
        }
        
        // Test Rose Gift x3 Combo (G key) → sút ra 3 quả bóng cùng lúc
        if (keyboard.gKey.wasPressedThisFrame) {
            if (wsClient != null)
            {
                // Random chọn 1 trong 5 user
                int randomUserIndex = Random.Range(0, testUsers.Length);
                string selectedUser = testUsers[randomUserIndex];
                
                // Simulate Rose Gift x3 (combo) → 1 entry trong queue, sút ra 3 quả
                wsClient.SimulateRoseGift(selectedUser, 3);
                
                Debug.Log($"[TiktokReceiverTest] 🌹x3 KEY PRESSED: G → Rose Gift x3 COMBO from {selectedUser} (will shoot 3 balls at once!)");
            }
            else
            {
                Debug.LogWarning("[TiktokReceiverTest] WebSocketClient is null!");
            }
        }
        
        // Test Perfume Gift (P key)
        if (keyboard.pKey.wasPressedThisFrame) {
            if (wsClient != null)
            {
                // Random chọn 1 trong 5 user (nhưng không hiển thị tên)
                int randomUserIndex = Random.Range(0, testUsers.Length);
                string selectedUser = testUsers[randomUserIndex];
                
                // Simulate Perfume Gift
                wsClient.SimulatePerfumeGift(selectedUser);
                
                Debug.Log($"[TiktokReceiverTest] 💐 KEY PRESSED: P → Perfume Gift (anonymous)");
            }
            else
            {
                Debug.LogWarning("[TiktokReceiverTest] WebSocketClient is null!");
            }
        }
        
        // Cập nhật UI liên tục nếu có
        if (textCountHeart != null && heartManager != null)
        {
            textCountHeart.text = heartManager.GetCurrentHeartCount().ToString();
        }
        
        // Cập nhật tên winner từ HeartManager nếu có
        if (currentNameSuperKick != null && heartManager != null)
        {
            string winnerName = heartManager.GetSelectedUserName();
            if (!string.IsNullOrEmpty(winnerName))
            {
                currentNameSuperKick.text = winnerName;
            }
        }
        
        // Enable/Disable currentNameSuperKick dựa trên trạng thái Super Kick
        if (currentNameSuperKick != null)
        {
            bool isSuperKickActive = false;
            
            // Check Super Kick status từ MatchManager
            if (MatchManager.Current != null)
            {
                isSuperKickActive = MatchManager.Current.IsSuperKick;
            }
            
            // Enable text khi Super Kick active, disable khi không
            currentNameSuperKick.enabled = isSuperKickActive;
            
            // Clear tên khi Super Kick kết thúc
            if (!isSuperKickActive && heartManager != null)
            {
                heartManager.ClearSelectedUserName();
            }
        }
        
        // Test command string (1 key)
        if (keyboard.digit1Key.wasPressedThisFrame) {
            Debug.Log("[TiktokReceiverTest] ⚡ KEY PRESSED: 1");
            Debug.Log("[TiktokReceiverTest] ⚡ Testing command: 'superkick'");
            receiver.OnTikTokCommand("superkick");
        }
        
        // Test command string (2 key)
        if (keyboard.digit2Key.wasPressedThisFrame) {
            Debug.Log("[TiktokReceiverTest] ⚡ KEY PRESSED: 2");
            Debug.Log("[TiktokReceiverTest] ⚡ Testing command: 'call5enemy'");
            receiver.OnTikTokCommand("call5enemy");
        }
    }
    
    void OnGUI() {
        if (!showOnScreenInstructions) return;
        
        GUI.color = Color.white;
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;
        
        int currentCount = heartManager != null ? heartManager.GetCurrentHeartCount() : 0;
        bool isSuperKickActive = heartManager != null ? heartManager.IsSuperKickActive() : false;
        int queueCount = heartManager != null ? heartManager.GetQueueCount() : 0;
        
        string instructions = 
            "=== TIKTOK RECEIVER TEST ===\n" +
            $"{superKickKey} → Super Kick x1 (direct)\n" +
            $"{call5EnemyKey} → Call 5 Enemy\n" +
            "U → Add Heart (random user)\n" +
            "R → Rose Gift x1 (1 ball)\n" +
            "G → Rose Gift x3 COMBO (3 balls)\n" +
            "O → Rose Gift x5 COMBO (5 balls)\n" +
            "P → Perfume Gift (Call5Enemy)\n" +
            $"    💖 Hearts: {currentCount}/100 (always active)\n" +
            $"    📋 Queue: {queueCount} entry waiting\n" +
            $"    {(isSuperKickActive ? "⚡ SUPER KICK ACTIVE" : "✅ Idle")}\n" +
            "1 → 'superkick' | 2 → 'call5enemy'";
        
        GUI.Box(new Rect(10, 10, 350, 240), instructions, style);
    }
}
