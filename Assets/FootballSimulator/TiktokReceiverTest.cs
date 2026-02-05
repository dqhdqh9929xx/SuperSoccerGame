using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

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
    
    [Header("UI")]
    [Tooltip("Hiện hướng dẫn trên màn hình")]
    public bool showOnScreenInstructions = true;
    
    [Header("TikTok Heart Test")]
    [Tooltip("TextMeshProUGUI để hiển thị số heart từ TikTok viewer")]
    public TextMeshProUGUI textCountHeart;
    
    [Tooltip("Số heart cần để trigger Super Kick")]
    public int heartThreshold = 100;
    
    private int countHeart = 0;
    
    void Start() {
        Debug.Log("[TiktokReceiverTest] ===== STARTING =====");
        
        receiver = FindObjectOfType<TiktokReceiver>();
        
        if (receiver == null) {
            Debug.LogError("[TiktokReceiverTest] ❌ TiktokReceiver NOT FOUND in scene!");
            Debug.LogError("[TiktokReceiverTest] Please create a GameObject with TiktokReceiver component!");
        } else {
            Debug.Log($"[TiktokReceiverTest] ✅ TiktokReceiver FOUND!");
            Debug.Log($"[TiktokReceiverTest] ✅ Ready! Press {superKickKey} for Super Kick, {call5EnemyKey} for Call 5 Enemy");
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
            return;
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
        
        // Test TikTok Heart System (U key để simulate heart từ viewer)
        if (keyboard.uKey.wasPressedThisFrame) {
            countHeart += 10;
            Debug.Log($"[TiktokReceiverTest] 💖 Heart received! Count: {countHeart}/{heartThreshold}");
            
            // Cập nhật UI text nếu có
            if (textCountHeart != null) {
                textCountHeart.text = countHeart.ToString();
            }
        }
        
        // Khi đủ heart → Trigger Super Kick
        if (countHeart >= heartThreshold) {
            Debug.Log($"[TiktokReceiverTest] 🔥 Heart threshold reached! Triggering Super Kick!");
            countHeart = 0;
            
            // Cập nhật UI
            if (textCountHeart != null) {
                textCountHeart.text = countHeart.ToString();
            }
            
            receiver.TriggerSuperKick();
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
        
        string instructions = 
            "=== TIKTOK RECEIVER TEST ===\n" +
            $"{superKickKey} → Super Kick\n" +
            $"{call5EnemyKey} → Call 5 Enemy\n" +
            "U → Add Heart (test viewer)\n" +
            $"    💖 Count: {countHeart}/{heartThreshold}\n" +
            "1 → Command: 'superkick'\n" +
            "2 → Command: 'call5enemy'\n" +
            "\n" +
            "Original keys (DISABLED):\n" +
            "I → DISABLED";
        
        GUI.Box(new Rect(10, 10, 300, 180), instructions, style);
    }
}
