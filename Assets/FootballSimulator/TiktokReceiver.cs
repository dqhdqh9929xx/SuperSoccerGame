using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FStudio.MatchEngine;

/// <summary>
/// Receiver để nhận event từ TikTok (hoặc nguồn bên ngoài) và trigger các tính năng trong game
/// Hỗ trợ 2 tính năng:
/// - SuperKick (U): Cú sút siêu mạnh
/// - Call5Enemy (I): Sinh 5 cầu thủ AI tấn công
/// </summary>
public class TiktokReceiver : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("Hiện log debug khi nhận event")]
    public bool showDebugLogs = true;
    
    private Call5EnemyManager call5EnemyManager;
    
    void Start() {
        // Tìm Call5EnemyManager trong scene
        if (MatchManager.Current != null) {
            call5EnemyManager = MatchManager.Current.GetComponent<Call5EnemyManager>();
            
            if (call5EnemyManager == null) {
                Debug.LogWarning("[TiktokReceiver] Call5EnemyManager not found on MatchManager");
            } else {
                if (showDebugLogs) {
                    Debug.Log("[TiktokReceiver] Call5EnemyManager reference cached successfully");
                }
            }
        }
    }
    
    void Update() {
        // Tự động cập nhật reference nếu chưa có
        if (call5EnemyManager == null && MatchManager.Current != null) {
            call5EnemyManager = MatchManager.Current.GetComponent<Call5EnemyManager>();
        }
    }
    
    /// <summary>
    /// Trigger Super Kick feature (phím U)
    /// Cú sút siêu mạnh về phía khung thành Home
    /// </summary>
    public void OnSuperKickEvent() {
        if (showDebugLogs) {
            Debug.Log("[TiktokReceiver] Received SuperKick event from TikTok");
        }
        
        if (MatchManager.Current == null) {
            Debug.LogWarning("[TiktokReceiver] Cannot trigger SuperKick - MatchManager is null");
            return;
        }
        
        MatchManager.Current.SetSuperKick(true);
        
        if (showDebugLogs) {
            Debug.Log("[TiktokReceiver] SuperKick activated!");
        }
    }
    
    /// <summary>
    /// Trigger Call5Enemy feature (phím I)
    /// Sinh 5 cầu thủ AI tấn công vào sân
    /// </summary>
    public void OnCall5EnemyEvent() {
        if (showDebugLogs) {
            Debug.Log("[TiktokReceiver] Received Call5Enemy event from TikTok");
        }
        
        if (call5EnemyManager == null) {
            Debug.LogWarning("[TiktokReceiver] Cannot trigger Call5Enemy - Call5EnemyManager is null");
            return;
        }
        
        call5EnemyManager.TriggerCall5Enemy();
        
        if (showDebugLogs) {
            Debug.Log("[TiktokReceiver] Call5Enemy triggered!");
        }
    }
    
    // ===== CONVENIENCE METHODS =====
    // Các method này có thể được gọi từ UnityEvent hoặc API khác
    
    /// <summary>
    /// Alias cho OnSuperKickEvent (để dễ gọi từ UI Button hoặc UnityEvent)
    /// </summary>
    public void TriggerSuperKick() {
        OnSuperKickEvent();
    }
    
    /// <summary>
    /// Alias cho OnCall5EnemyEvent (để dễ gọi từ UI Button hoặc UnityEvent)
    /// </summary>
    public void TriggerCall5Enemy() {
        OnCall5EnemyEvent();
    }
    
    /// <summary>
    /// Trigger event dựa trên string command (cho WebGL hoặc external API)
    /// </summary>
    /// <param name="command">"superkick" hoặc "call5enemy"</param>
    public void OnTikTokCommand(string command) {
        if (showDebugLogs) {
            Debug.Log($"[TiktokReceiver] Received command: {command}");
        }
        
        switch (command.ToLower()) {
            case "superkick":
            case "super_kick":
            case "u":
                OnSuperKickEvent();
                break;
                
            case "call5enemy":
            case "call_5_enemy":
            case "i":
                OnCall5EnemyEvent();
                break;
                
            default:
                Debug.LogWarning($"[TiktokReceiver] Unknown command: {command}");
                break;
        }
    }
}
