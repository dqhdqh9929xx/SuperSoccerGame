using UnityEngine;
using UnityEngine.InputSystem;
using FStudio.MatchEngine;

/// <summary>
/// Hiển thị status debug trên màn hình để dễ kiểm tra
/// </summary>
public class DebugStatusDisplay : MonoBehaviour {
    [Header("Settings")]
    public bool showDebugPanel = true;
    public KeyCode toggleKey = KeyCode.F1;
    
    private TiktokReceiver receiver;
    private Call5EnemyManager call5Manager;
    private string lastKeyPressed = "None";
    private float lastKeyPressedTime = 0f;
    
    void Start() {
        Debug.Log("[DebugStatusDisplay] ✅ Started! Press F1 to toggle debug panel");
    }
    
    void Update() {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Toggle panel (F1 key)
        if (keyboard.f1Key.wasPressedThisFrame) {
            showDebugPanel = !showDebugPanel;
            Debug.Log($"[DebugStatusDisplay] Debug panel: {(showDebugPanel ? "ON" : "OFF")}");
        }
        
        // Tìm references
        if (receiver == null) {
            receiver = FindObjectOfType<TiktokReceiver>();
        }
        
        if (call5Manager == null && MatchManager.Current != null) {
            call5Manager = MatchManager.Current.GetComponent<Call5EnemyManager>();
        }
        
        // Track last key pressed
        if (keyboard.anyKey.wasPressedThisFrame) {
            // Check some common keys
            if (keyboard.tKey.wasPressedThisFrame) lastKeyPressed = "T";
            else if (keyboard.yKey.wasPressedThisFrame) lastKeyPressed = "Y";
            else if (keyboard.uKey.wasPressedThisFrame) lastKeyPressed = "U";
            else if (keyboard.iKey.wasPressedThisFrame) lastKeyPressed = "I";
            else if (keyboard.digit1Key.wasPressedThisFrame) lastKeyPressed = "1";
            else if (keyboard.digit2Key.wasPressedThisFrame) lastKeyPressed = "2";
            else if (keyboard.f1Key.wasPressedThisFrame) lastKeyPressed = "F1";
            else lastKeyPressed = "Some Key";
            
            lastKeyPressedTime = Time.time;
        }
    }
    
    void OnGUI() {
        if (!showDebugPanel) {
            // Show minimal hint
            GUI.color = Color.white;
            GUI.Label(new Rect(10, Screen.height - 30, 300, 30), $"Press {toggleKey} for Debug Panel");
            return;
        }
        
        // Main debug panel
        GUI.color = Color.white;
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.fontSize = 12;
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.normal.textColor = Color.white;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);
        
        string status = "=== DEBUG STATUS ===\n\n";
        
        // Component Status
        status += "📦 COMPONENTS:\n";
        status += $"  TiktokReceiver: {(receiver != null ? "✅ FOUND" : "❌ NOT FOUND")}\n";
        status += $"  Call5EnemyManager: {(call5Manager != null ? "✅ FOUND" : "❌ NOT FOUND")}\n";
        status += $"  MatchManager: {(MatchManager.Current != null ? "✅ ACTIVE" : "❌ NULL")}\n";
        status += "\n";
        
        // Input Status
        status += "⌨️ INPUT:\n";
        status += $"  Last Key: {lastKeyPressed}\n";
        float timeSinceKey = Time.time - lastKeyPressedTime;
        if (timeSinceKey < 2f) {
            status += $"  ⚡ Pressed {timeSinceKey:F1}s ago\n";
        }
        status += "\n";
        
        // Match Status
        if (MatchManager.Current != null) {
            status += "⚽ MATCH STATUS:\n";
            status += $"  Status: {MatchManager.Current.MatchFlags}\n";
            status += $"  SuperKick: {(MatchManager.Current.IsSuperKick ? "🔥 ACTIVE" : "Inactive")}\n";
        } else {
            status += "⚽ MATCH STATUS:\n";
            status += "  ❌ NOT IN MATCH\n";
            status += "  → Start a match first!\n";
        }
        status += "\n";
        
        // Test Keys
        status += "🎮 TEST KEYS:\n";
        status += "  T → Super Kick\n";
        status += "  Y → Call 5 Enemy\n";
        status += "  1 → Command 'superkick'\n";
        status += "  2 → Command 'call5enemy'\n";
        status += $"  {toggleKey} → Toggle this panel\n";
        
        GUI.Box(new Rect(10, 10, 350, 350), status, boxStyle);
        
        // Warning if not in match
        if (MatchManager.Current == null) {
            GUI.color = Color.yellow;
            GUIStyle warningStyle = new GUIStyle(GUI.skin.box);
            warningStyle.fontSize = 14;
            warningStyle.alignment = TextAnchor.MiddleCenter;
            warningStyle.normal.textColor = Color.yellow;
            warningStyle.fontStyle = FontStyle.Bold;
            
            GUI.Box(new Rect(10, 370, 350, 60), "⚠️ WARNING\nStart a match to test features!", warningStyle);
        }
    }
}
