using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using FStudio.MatchEngine;

/// <summary>
/// Test script cho TiktokReceiver
/// Bấm phím T → Trigger Super Kick
/// Bấm phím Y → Trigger Call 5 Enemy
/// Bấm phím U → Tăng Heart count (test TikTok viewer)
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
        
        string instructions = 
            "=== TIKTOK RECEIVER TEST ===\n" +
            $"{superKickKey} → Super Kick\n" +
            $"{call5EnemyKey} → Call 5 Enemy\n" +
            "U → Add Heart (random user)\n" +
            $"    💖 Count: {currentCount}/100\n" +
            $"    {(isSuperKickActive ? "⛔ SUPER KICK ACTIVE" : "✅ Tap enabled")}\n" +
            "1 → Command: 'superkick'\n" +
            "2 → Command: 'call5enemy'";
        
        GUI.Box(new Rect(10, 10, 300, 180), instructions, style);
    }
}
